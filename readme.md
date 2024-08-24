# **KeyBoard-Glance**

 KB-Glance is a WPF application designed to help you learn and visualize your custom ZMK keymap. The app listens for specific key combinations (combo keys) that you define and displays the corresponding keymap layout image for the active layer. This is particularly useful for users who frequently switch between different layers in their keymap and want a quick reference to their current layout.

## **Features**

- **Configurable Key Listening:** Define and configure combo keys that trigger different layers in your ZMK keymap.
- **Layer Visualization:** Displays the keymap layout for the active layer based on your custom images.

## **Installation and Setup**

### **Prerequisites**

- Windows Operating System

### **Step-by-Step Installation**

1. **Download the Release**

   - Download the latest version of the app from the [Releases](https://github.com/CornillieJ/KBGlance/releases) section.
   - Extract the ZIP file to your preferred location.

2. **Prepare the Keymap Images**

   - Navigate to the `Resources` folder within the extracted directory.
   - Add your keymap images to this folder, ensuring they are named according to the following convention:
     ```
     layer_0.png
     layer_1.png
     layer_2.png
     ...
     ```
   - The numbers correspond to the layer numbers in your ZMK keymap.
   - I recommend using one of the visualizing tools you can find online for this. I personally just took screenshots of the amazing online keymap editor at https://nickcoutsos.github.io/keymap-editor/

3. **Configure Combo Keys in ZMK**

   - To use this application effectively, you need to configure combo keys in your ZMK keymap that trigger specific layers.
   - **Recommended Setup:** Map your left pinky and index finger keys to trigger a combination of `RALT + RCTRL + <layer_number>`.
   - This should be defined on **each layer** of your ZMK keymap, with each layer having a unique number key combination (e.g., `RALT + RCTRL + 0` for layer 0, `RALT + RCTRL + 1` for layer 1, etc.).  use: https://zmk.dev/docs/keymaps/combos
   
   - Example configuration in your ZMK keymap:
     ```c
     combos {
         Show_layer_0 {
            timeout-ms = <100>;
            bindings = <&kp RALT(RCTRL(N0))>;
            key-positions = <13 16>;
            layers = <0>;
            slow-release;
        };

        Show_layer_1 {
            timeout-ms = <100>;
            bindings = <&kp RALT(RCTRL(N1))>;
            key-positions = <13 16>;
            layers = <1>;
            slow-release;
        };
         // Repeat for each layer...
     };
     ```
4. **Configure Combo Keys in the App**

   - Launch the application by double-clicking on the `keyboardglance.exe` file.
   - this opens the configuration window where you can define the combo keys that match your ZMK keymap settings.
   - Pressing start will save your configuration.

5. **Run the Application**

   - After pressing start, the app will minimize to system tray, and it will listen for the configured combo keys.
   - Upon detecting a combo key press, the application will display the corresponding keymap layout image.

## **Usage**

### **Adding Keymap Images**

- **Manually Adding Images:**
  - Place your keymap layout images in the `Resources` folder.
  - Ensure each image is correctly named (`layer_0.png`, `layer_1.png`, etc.) to match your layer numbers.

### **Configuring Combo Keys**

- Open the configuration section in the app.
- Define the combo keys you use to trigger different layers, ensuring they match the combos you configured in ZMK.
- The app will listen for these key combinations and update the displayed keymap accordingly.

### **Switching Layers**

- Press the combo keys (e.g., `RALT + RCTRL + 1`) on your keyboard.
- The app will automatically detect the right combo and change the displayed image to reflect the keymap for the active layer.

## **Future Enhancements**

- **Allow choosing location and size of the popups:**
  - In the future, I plan to add functionality to allow customizing the popups, an easy task but I'd need to actually take time to update it 

- **Image Uploading:**
  -I would like to streamline the image process and then worki on an easy-to-use image uploading feature that will automatically rename and place images in the correct directory.

## **Contributing**

Contributions are welcome! If you have ideas, bug reports, or improvements, feel free to open an issue or submit a pull request.