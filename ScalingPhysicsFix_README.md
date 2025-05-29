# Beyblade Physics Scaling Fix

## Problem
When scaling Unity GameObjects down to small sizes (e.g., 0.1x), several physics issues occur:

1. **Rapid Slowdown**: Objects lose momentum quickly and come to a stop
2. **Wobbling**: Instead of toppling naturally, objects exhibit unrealistic wobbling
3. **Premature Sleep**: Physics objects go to sleep too quickly
4. **Incorrect Inertia**: The inertia tensor doesn't scale properly with the object

## Solution
The updated `BeybladePhysicsSetup.cs` script includes scale-aware physics adjustments:

### Key Features

#### 1. Automatic Scale Detection
- Detects the current scale of the GameObject
- Calculates scale ratio relative to a base scale (default: 1.0)

#### 2. Inertia Tensor Correction
- Applies custom inertia tensor values
- Scales inertia appropriately (scales with scale²)
- Prevents unrealistic rotation behavior

#### 3. Sleep Threshold Adjustment
- Reduces sleep threshold for smaller objects
- Prevents premature physics sleeping
- Keeps small objects active longer

#### 4. Drag Adjustment
- Sets appropriate angular and linear drag values
- Prevents unrealistic energy loss
- Maintains realistic motion at all scales

#### 5. Scale-Aware Physics Materials
- Optional `ScaleAwarePhysicsMaterial` scriptable object
- Automatically adjusts friction and bounciness based on scale
- Provides consistent surface interaction at all scales

### Setup Instructions

#### Basic Setup
1. Add the `BeybladePhysicsSetup` component to your beyblade GameObject
2. Ensure it has a Rigidbody component
3. Configure the "Scale-Aware Physics" settings:
   - Enable "Auto Adjust For Scale"
   - Set "Base Scale" to your reference scale (usually 1.0)
   - Adjust "Custom Inertia Tensor" values for your object
   - Fine-tune drag values if needed

#### Advanced Setup with Physics Materials
1. Create a scale-aware physics material:
   - Right-click in Project → Create → Physics → Scale-Aware Physics Material
   - Configure base friction and bounciness values
   - Set up scale adjustment curves
2. Assign the material to the "Scale Aware Material" field in BeybladePhysicsSetup

### Configuration Parameters

#### Scale-Aware Physics Settings
- **Auto Adjust For Scale**: Enable/disable automatic scale adjustments
- **Base Scale**: The scale these settings are designed for (reference scale)
- **Custom Inertia**: Enable custom inertia tensor
- **Custom Inertia Tensor**: Custom inertia values (X, Y, Z)
- **Sleep Threshold Multiplier**: Controls how hard it is for the object to sleep
- **Angular Drag Override**: Angular drag value for consistent rotation
- **Linear Drag Override**: Linear drag value for consistent movement
- **Scale Aware Material**: Optional physics material that adjusts with scale

### Recommended Values for Beyblades

#### For 0.1x Scale
- Custom Inertia Tensor: (0.1, 0.2, 0.1)
- Sleep Threshold Multiplier: 0.1
- Angular Drag Override: 0.05
- Linear Drag Override: 0.1

#### For 0.5x Scale
- Custom Inertia Tensor: (0.05, 0.1, 0.05)
- Sleep Threshold Multiplier: 0.2
- Angular Drag Override: 0.03
- Linear Drag Override: 0.08

### Debugging and Visualization

The script provides extensive debugging information:
- Scale ratio calculations
- Adjusted physics values
- Real-time visualization in Scene view
- Console logs with detailed physics settings

#### Scene View Visualization
- Shows center of mass with scale information
- Displays adjusted physics values during play mode
- Shows torque direction and magnitude
- Real-time angular velocity display

### Troubleshooting

#### Object Still Slows Down Too Fast
- Reduce Angular Drag Override (try 0.01-0.03)
- Reduce Sleep Threshold Multiplier (try 0.05)
- Check if physics materials have too much friction

#### Object Wobbles Instead of Toppling
- Adjust Custom Inertia Tensor values
- Try increasing Y-axis inertia relative to X and Z
- Ensure center of mass is properly configured

#### Object Doesn't Respond to Forces
- Check scale ratio calculations in console
- Verify forces are being scaled appropriately
- Ensure Rigidbody is not kinematic

### Technical Details

#### Inertia Tensor Scaling
Inertia tensor scales with the square of the scale factor:
```
scaledInertia = customInertia * (scaleRatio²)
```

#### Angular Velocity Scaling
Maximum angular velocity is inversely proportional to scale:
```
scaledMaxAngularVelocity = maxAngularVelocity * (1 / scaleRatio)
```

#### Force Scaling
Applied forces are scaled proportionally:
```
scaledForce = originalForce * scaleRatio
```

This ensures consistent behavior across all scales while maintaining realistic physics. 