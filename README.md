<p align="center">
  <img width="50%" alt="WebcamPoseTracker" src="https://github.com/user-attachments/assets/ce83c23a-76e9-490f-8f32-4d9e48746494" />
</p>

## Feature

Using Mediapipe to track pose in Unity.

## Install
Follow the steps below.

1. This repositories clone please.
    ```bash
    https://github.com/NullClone/WebcamPoseTracker.git
    ```

2. Open in Unity `6000.0.43f1`

## Usage
Open the scene from `Assets > Scenes > PoseTracking`

## Explanation

### Inference Runner
<p align="center">
 <img width="40%" alt="Inference Runner" src="https://github.com/user-attachments/assets/3d40e08a-751b-402a-b72c-70ffd8c4820d" />
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
  </tbody>
</table>

## Roadmap
- [x] Implementing Kalman Filter
- [x] Implementing LowPass Filter 
- [ ] Multi Person Tracking
