import cv2
import mediapipe as mp
import socket
import time
import os
import math
from collections import deque

# --- NETWORK SETUP ---
UDP_IP = "127.0.0.1"  
UDP_PORT = 5005       
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# --- OVERHEAD STREAM ---
DROIDCAM_URL = 1  

# --- CORE TASK API ---
BaseOptions = mp.tasks.BaseOptions
HandLandmarker = mp.tasks.vision.HandLandmarker
HandLandmarkerOptions = mp.tasks.vision.HandLandmarkerOptions
VisionRunningMode = mp.tasks.vision.RunningMode

MODEL_PATH = "hand_landmarker.task"

if not os.path.exists(MODEL_PATH):
    print(f"[ERROR] Missing '{MODEL_PATH}'!")
    exit()

options = HandLandmarkerOptions(
    base_options=BaseOptions(model_asset_path=MODEL_PATH),
    running_mode=VisionRunningMode.VIDEO,
    num_hands=2
)

# --- WAVE TUNING PARAMETERS ---
position_history = deque(maxlen=5)
last_swipe_time = 0
COOLDOWN = 0.5            
SWIPE_VELOCITY_LIMIT = 0.25 

def is_hand_splayed(hand_landmarks):
    """
    Checks if the index, middle, ring, and pinky fingers are fully extended.
    Compares fingertip distances to their respective knuckle bases.
    """
    # Landmark pairs: (Fingertip, Knuckle Base)
    finger_tips_and_bases = [
        (8, 5),   # Index Finger
        (12, 9),  # Middle Finger
        (16, 13), # Ring Finger
        (20, 17)  # Pinky Finger
    ]
    
    extended_count = 0
    
    # Calculate approximate distance for each finger
    for tip_idx, base_idx in finger_tips_and_bases:
        tip = hand_landmarks[tip_idx]
        base = hand_landmarks[base_idx]
        
        # 2D Euclidean distance in normalized screen space
        distance = math.sqrt((tip.x - base.x)**2 + (tip.y - base.y)**2)
        
        # If the fingertip is far from the knuckle base, it's extended
        if distance > 0.08:  # Adjust this threshold if your hands are small/large on screen
            extended_count += 1
            
    # Return True only if all 4 fingers are extended/splayed open
    return extended_count == 4

print("[INFO] Tracking Engine Active with Finger Splay Filter.")
cap = cv2.VideoCapture(DROIDCAM_URL)
cap.set(cv2.CAP_PROP_BUFFERSIZE, 1)

with HandLandmarker.create_from_options(options) as landmarker:
    while cap.isOpened():
        success, frame = cap.read()
        if not success:
            continue

        frame = cv2.flip(frame, 1)
        h, w, _ = frame.shape
        
        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb_frame)
        
        frame_timestamp_ms = int(time.time() * 1000)
        detection_result = landmarker.detect_for_video(mp_image, frame_timestamp_ms)

        hand_detected_this_frame = False

        if detection_result and hasattr(detection_result, 'hand_landmarks') and detection_result.hand_landmarks:
            for hand_landmarks in detection_result.hand_landmarks:
                if len(hand_landmarks) >= 21:
                    hand_detected_this_frame = True
                    
                    # FIRST CHECK: Is the hand actually splayed open?
                    if not is_hand_splayed(hand_landmarks):
                        # Hand is pinching tweezers or closed -> clear trail and ignore gestures
                        position_history.clear()
                        continue
                    
                    # SECOND CHECK: Track velocity if open
                    wrist = hand_landmarks[0]
                    current_x = wrist.x 
                    current_time = time.time()
                    
                    position_history.append((current_x, current_time))

                    if len(position_history) == position_history.maxlen and (current_time - last_swipe_time > COOLDOWN):
                        start_x, start_t = position_history[0]
                        end_x, end_t = position_history[-1]
                        
                        dt = end_t - start_t
                        if dt > 0:
                            velocity = (end_x - start_x) / dt

                            if velocity > SWIPE_VELOCITY_LIMIT:
                                print("[➔ GESTURE] Open Hand Right Wave Sent!")
                                sock.sendto(b"WAVE_RIGHT", (UDP_IP, UDP_PORT))
                                last_swipe_time = current_time
                                position_history.clear()
                            
                            elif velocity < -SWIPE_VELOCITY_LIMIT:
                                print("[➔ GESTURE] Open Hand Left Wave Sent!")
                                sock.sendto(b"WAVE_LEFT", (UDP_IP, UDP_PORT))
                                last_swipe_time = current_time
                                position_history.clear()
        
        if not hand_detected_this_frame:
            position_history.clear()

        cv2.imshow('Overhead ML Workspace (Press Q to exit)', frame)
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

cap.release()
cv2.destroyAllWindows()