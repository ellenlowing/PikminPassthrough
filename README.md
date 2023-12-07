# PikminPassthrough

This is a project exploring how existing 3D game mechanics can be borrowed and adapted to create more intuitive and immersive gameplay in mixed reality. Using the Pikmin game series as an example, I reproduced various classic interactions in passthrough, such as [Throw](https://www.pikminwiki.com/Throw) and [Pluck](https://www.pikminwiki.com/Pluck), that introduce some alternative interaction patterns in XR. 

It's still very much a WIP 🚧 Here are some future mechanics I'd like to introduce:

- Whistle to gather wild pikmins
- Inverse kinematics to swing pikmin around fluidly with hand
- Constructions 

## Demo

![Pikmin follows in pack](./Demo/pikmin-tube-multicolor.gif)

## Technical Details

I built and tested this demo with Unity and Meta Quest 3. I used Oculus API to handle the interactions and occlusion. The main scene is in `Assets/Scenes/PikminPack` and the majority of code for the main demo is in `Assets/Pikmin/Scripts/PikminPack`.

In the experience, a pack of pikmins will follow you (Leader) in a circular formation when you walk far enough from the pack. Since the Leader position is only deduced from the headset's world position, which is constantly moving, the program ignores any changes in the Y-axis and filters out movements that are too small from the origin of the pack. 

To save build time on the device, a lot of prototyping was done in the Unity editor. Because Quest doesn't officially support the Device Simulator on a Mac, I created a mock scene that contains planes that have the same orientation as OVRPlane, which are GameObjects that Quest generates for detected planes in passthrough. 

## Resources

Pikmin models and animations are provided by [The Models Resource](https://www.models-resource.com/3ds/photoswithpikmin/model/20053/) website.
