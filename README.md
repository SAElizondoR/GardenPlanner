# GardenPlanner - Augmented Reality project

## Introduction
This is an augmented reality application for android phones for Garden Planning. 
The user can select and place various objects, scale them, rotate, move, delete.
It is also possible to collaborate with other user, by synchronizing the scenes on multiple devices.

You can see how the application works in the AR_VR_video.mp4 file.

## Dependencies
- AR Foundation
- ARCore

## Running the Project
The application was developed with Unity 2021.3.14f1.
You can also run the project on your android phone by installing the garden_planner.apk file. 
Make sure that the deviece supports ARCore. Minimum API level: Android 8.0 'Oreo' (API level 26).

## Specifics

### AR Foundation
AR Foundation was used as the base of the Augmented reality application
- image tracking 
- plane detection
- raycast (so we can detect if an object was tapped)
- anchors
- position, rotation, scale calculations

### Photon Unity Networking
Photon Unity Networking was used for connecting the phones for collaboration
- Create/join room
- Photon View
- Remote Procedure Calls (RPC)
- Network object instances
- Photon Transfowm View

### Lean Touch
Lean Touch was used for rotating and scaling the objects. We used a custom translation script.
- Lean Twist Rotate
- Lean Pinch Scale
- Lean Selectable by Finger

### Functionalities
- connecting phones so they see the same scene
- selecting object
- placing object in scene
- rotating, scaling, moving objects
- removing objects
- turning off and on plane detection
