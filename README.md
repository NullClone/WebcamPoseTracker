> [!WARNING]
> This repository has been archived as further improvement is needed.
The new repository is [here](https://github.com/NullClone/PrismMotionTracker).

## Feature
Using Mediapipe to track pose in Unity.

<img width="50%" alt="WebcamPoseTracker" src="https://github.com/user-attachments/assets/0f561d3e-f4e3-4832-916d-f1e129039bae" />

## Install
Follow the steps below.

1. This repositories clone please.
    ```bash
    https://github.com/NullClone/WebcamPoseTracker.git
    ```

2. Open in Unity `6000.0.43f1`

## Usage
Open the scene from `Assets > Scenes > PoseTracking`

> [!WARNING]
> If you use a webcam, don't forget to change the `SourceType` of `ImageSource` to `Webcam` and set the device.

## Explanation

### Inference Runner
<p align="center">
  <img width="50%" alt="Inference Runner" src="https://github.com/user-attachments/assets/b1874985-5033-45f7-8961-6a4242d12cf1" />
</p>

<table width="100%">
  <thead>
    <tr>
      <td colspan="3">
        <b>Property Name</b>
      </td>
      <td>
        <b>Description</b>
      </td>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td colspan="3">
        <b>Image Source</b>
      </td>
      <td>
        Get the output for the image source.
      </td>     
    </tr>
    <tr>
      <td colspan="3">
        <b>Performance Level</b>
      </td>
      <td>
        <p>You can specify the performance level.</p>
        <ul>
          <li>Lite</li>
          <li>Full (Default)</li>
          <li>Heavy</li>
        </ul>
        <strong>Selecting Heavy may slow down operation.</strong>
      </td>
    </tr>
    <tr>
      <td colspan="3">
        <b>Backend Type</b>
      </td>
      <td>
        <p>Types of backend uses to execute a neural network. </p>
        <ul>
          <li>CPU</li>
          <li>GPUCompute (Default)</li>
          <li>GPUPixel</li>
        </ul>
      </td>
    </tr>
    <tr>
      <td colspan="3">
        <b>Score Threshold</b>
      </td>
      <td>
        <p>Filter results based on prediction scores.</p>
      </td>
    </tr>
    <tr>
      <td colspan="3">
        <b>Filter</b>
      </td>
      <td>
        <p>You can specify the filter.</p>
        <ul>
          <li>None</li>
          <li>Kalman Filter (Default)</li>
          <li>Low Pass Filter (Default)</li>
        </ul>
      </td>
    </tr>
    <tr>
      <td>-</td>
      <td colspan=2>
        <b>Kalman Filter</b>
      </td>
      <td>   
        <b>Time Interval<br></b>
        <p>Kalman filter time interval.</p>
        <p>
          Higher values will result in more agile movement but more jerky movements.<br>
          Lower values will result in smoother movement but less movement.<br>
        </p>
        <b>Noise<br></b>
        <p>The noise magnitude of the Kalman filter.</p>
      </td>
    </tr>
    <tr>
      <td>-</td>
      <td colspan=2>
        <b>Low Pass Filter</b>
      </td>
      <td>
        <b>Smooth<br></b>
        <p>Low Pass Filter smoothness.</p>
        <p>Changing the value does not affect the processing speed.</p>
        <b>N Order</b><br>
        <p>Enter the number of times to apply the Low Pass Filter.</p>
      </td>
    </tr>
    <tr>
      <td colspan=3>
        <b>Keypoints</b>
      </td>
      <td>
        <p>Keypoints for debugging.</p>
      </td>
    </tr>
  </tbody>
</table>

## Roadmap
- [x] Implementing Kalman Filter
- [x] Implementing Low Pass Filter 
- [ ] Multi Person Tracking

## Reference
- https://ai.google.dev/edge/mediapipe/solutions/vision/pose_landmarker
- https://digital-standard.com/tdpt/
