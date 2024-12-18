using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MobileScreen
{
    public partial class Form1 : Form
    {
        // Store initial form width and expanded width for resizing the form
        private int initialFormWidth;
        private int expandedFormWidth;

        // Timer for smooth resizing of the form
        private Timer resizeTimer;

        // The target width for the form during resize operation
        private int targetWidth;

        // Step size for resizing (adjust for speed of expansion/collapse)
        private int resizeStep = 28;

        // Flag to indicate whether the form is expanding or collapsing
        private bool expanding;

        // Variable to store the output of executed commands
        private string storedOutput = string.Empty;

        public Form1()
        {
            InitializeComponent();

            // Tooltips for UI elements to provide helpful hints to the user
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(label9, "Click to see ADB Commands list");
            toolTip.SetToolTip(labelToggleOutput, "Click to expand/collapse OUTPUT info");
            toolTip.SetToolTip(labelOpenOutput, "Click to export OUTPUT value");

            // Define initial and expanded widths for form resizing
            initialFormWidth = 234;  // Width for the form in collapsed state
            expandedFormWidth = 962; // Width for the form in expanded state

            // Set the initial form width
            this.Width = initialFormWidth;

            // Initially hide the output text box
            txtOutput.Visible = false;

            // Initialize the Timer for smooth transition of form width
            resizeTimer = new Timer();
            resizeTimer.Interval = 1; // Timer interval (lower = faster transition)
            resizeTimer.Tick += ResizeTimer_Tick; // Event handler for timer ticks

            // Subscribe to the TextChanged event for both textboxes
            txtIP.TextChanged += txtIP_TextChanged;
            txtIP2.TextChanged += txtIP2_TextChanged;
        }
        // When the first IP textbox changes, update the second one
        private void txtIP_TextChanged(object sender, EventArgs e)
        {
            // Avoid recursion (if the second textbox is updated, it will not update the first one)
            if (txtIP2.Text != txtIP.Text)
            {
                txtIP2.Text = txtIP.Text;
            }
        }

        // When the second IP textbox changes, update the first one
        private void txtIP2_TextChanged(object sender, EventArgs e)
        {
            // Avoid recursion (if the first textbox is updated, it will not update the second one)
            if (txtIP.Text != txtIP2.Text)
            {
                txtIP.Text = txtIP2.Text;
            }
        }

        // Method to execute a command in the command prompt and capture output or error
        private string RunCommand(string command)
        {
            try
            {
                // Initialize the process to execute the command
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c " + command, // "/c" runs the command and closes the cmd window
                        RedirectStandardOutput = true, // Redirect standard output
                        RedirectStandardError = true,  // Redirect error output
                        UseShellExecute = false,      // Prevent using the shell
                        CreateNoWindow = true         // Hide the command window
                    }
                };

                process.Start();

                // Read the output and error streams
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                // Return output or error depending on the result
                return string.IsNullOrEmpty(error) ? output : error;
            }
            catch (Exception ex)
            {
                // Return the exception message if an error occurs
                return $"Error running command: {ex.Message}";
            }
        }

        // Button click event to connect to an Android device using ADB
        private async void button1_Click(object sender, EventArgs e)
        {
            // Get IP and Port from the textboxes
            string ip = txtIP.Text.Trim();
            string port = txtPort.Text.Trim();

            // Validate input fields
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port))
            {
                MessageBox.Show("Please provide both IP and Port.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string adbCommand = $"adb connect {ip}:{port}";
            string scrcpyCommand = "scrcpy.exe";

            try
            {
                // Check if adb.exe and scrcpy.exe are available in the application directory
                if (!File.Exists("adb.exe"))
                {
                    MessageBox.Show("adb.exe not found in the application directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!File.Exists("scrcpy.exe"))
                {
                    MessageBox.Show("scrcpy.exe not found in the application directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Run the ADB connect command in the background
                string adbOutput = await Task.Run(() => RunCommand(adbCommand));
                if (!adbOutput.Contains("connected to"))
                {
                    MessageBox.Show($"ADB connection failed:\n{adbOutput}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Run scrcpy command to start screen mirroring
                string scrcpyOutput = await Task.Run(() => RunCommand(scrcpyCommand));

                // Handle scrcpy output for errors
                if (scrcpyOutput.Contains("Could not find any ADB device") || scrcpyOutput.Contains("Server connection failed"))
                {
                    MessageBox.Show($"scrcpy failed:\n{scrcpyOutput}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // Close the form after successful execution
                    //Close();
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                MessageBox.Show($"An error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Button click event to pair with an Android device using ADB
        private void button2_Click_1(object sender, EventArgs e)
        {
            // Get IP, Port, and Pairing Code from the textboxes
            string ip = txtIP2.Text.Trim();
            string port = txtPort2.Text.Trim();
            string pairingCode = txtPairCode.Text.Trim(); // Get the pairing code from the textbox

            // Validate IP, Port, and Pairing Code fields
            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(pairingCode))
            {
                MessageBox.Show("Please provide IP, Port, and CODE.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string adbPairCommand = $"adb pair {ip}:{port} {pairingCode}";

            try
            {
                // Check if adb.exe exists
                if (!File.Exists("adb.exe"))
                {
                    MessageBox.Show("adb.exe not found in the application directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Run the ADB pair command to pair the device
                string adbPairOutput = RunCommand(adbPairCommand); // This will keep the CMD open

                // Check if pairing was successful
                if (adbPairOutput.Contains("Successfully paired to "))
                {
                    MessageBox.Show("Device paired successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"adb pair failed:\n{adbPairOutput}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // Handle unexpected errors during pairing
                MessageBox.Show($"An error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Button click event to execute an ADB command
        private async void button3_Click(object sender, EventArgs e)
        {
            string adbCommand = txtCommand.Text.Trim();

            // Clear the output field and validate that the adb command is not empty
            if (string.IsNullOrEmpty(adbCommand))
            {
                txtOutput.Clear();
                MessageBox.Show("Please enter an adb command.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if adb.exe exists
            if (!File.Exists("adb.exe"))
            {
                MessageBox.Show("adb.exe not found in the application directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Run the ADB command and capture output asynchronously
                string commandOutput = await Task.Run(() => RunCommand(adbCommand));

                // Display the output or handle if no output is returned
                if (!string.IsNullOrEmpty(commandOutput))
                {
                    txtOutput.Text = commandOutput;
                    storedOutput = commandOutput;  // Store the output to be saved later
                }
                else
                {
                    MessageBox.Show("No output returned for the command.", "No Output", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                // Handle errors during command execution
                MessageBox.Show($"An error occurred while executing the command:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event handler to open the Commands.txt file
        private void label9_Click(object sender, EventArgs e)
        {
            string filePath = Path.Combine(Application.StartupPath, "Commands.txt"); // File in the same directory as the app

            // Check if the file exists and open it
            if (File.Exists(filePath))
            {
                Process.Start(filePath); // Open the file with default program (Notepad)
            }
            else
            {
                MessageBox.Show("The Commands.txt file was not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Show help topics in a message box
        private void topicsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Follow the steps below to get started quickly.\n\n" +
                            "- Connecting to an Android Device\n" +
                            "  1. Enter the IP Address and Port of your Android device in the provided fields.\n" +
                            "  2. Click Connect.\n" +
                            "The app will attempt to establish a connection with the Android device using the ADB connect command.\n" +
                            "If successful, the Android device screen will be mirrored on your PC using scrcpy.\n\n" +
                            "- Pairing with an Android Device\n" +
                            "  If you're connecting to a device for the first time, you may need to pair it:\n" +
                            "  1. Enter the IP Address, Port, and Pairing Code provided by the device.\n" +
                            "  2. Click Pair.\n" +
                            "The app will send the pairing code to the Android device, establishing a secure connection.\n" +
                            "Once paired successfully, the device will be ready for control and screen mirroring.\n\n" +
                            "- Running ADB Commands\n" +
                            "  1. Enter any ADB command in the Command text box.\n" +
                            "  2. Click Run Command.\n" +
                            "The app will execute the command, and any output or errors will be displayed in the app.\n" +
                            "You will see the command's output in the output box, which can help troubleshoot or confirm command execution.\n\n" +
                            "- Opening ADB Command List\n" +
                            "  Click on the Commands label (located in the app’s interface) to view a list of available ADB commands.\n" +
                            "This file is accessible from the Commands.txt file located in the app's directory.\n\n" +
                            "- Troubleshooting\n" +
                            "  - Ensure that adb.exe and scrcpy.exe are located in the same directory as the application.\n" +
                            "  - If the connection fails, double-check the IP Address and Port you entered.\n" +
                            "  - If scrcpy does not start, confirm that your Android device is properly connected and ready to mirror.\n\n" +
                            "- Help and Support\n" +
                            "  For any additional questions or concerns, please feel free to send a direct message via Teams.",
                            "Help Topics", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Display app version and details
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This app serves as a GUI wrapper for ADB and scrcpy tools, simplifying the process of connecting, pairing, and running commands for Android devices. It’s particularly useful for:\n\n" +
                            "- QA and Developers testing Android devices over a network.\n" +
                            "- Users who want a quick way to mirror or control their Android screens on a PC.\n\n" +
                            "Version 1.0\nCrafted by Stefan Antonijevic",
                            "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Toggle form width for expanding or collapsing the output section
        private void labelToggleOutput_Click(object sender, EventArgs e)
        {
            // Toggle form width based on current state
            if (this.Width == expandedFormWidth)
            {
                targetWidth = initialFormWidth; // Collapse the form
                expanding = false;
            }
            else
            {
                targetWidth = expandedFormWidth; // Expand the form
                expanding = true;
                txtOutput.Visible = true; // Show the output textbox when expanded
            }

            // Start the Timer to handle smooth resizing of the form
            resizeTimer.Start();
        }

        // Timer event handler to resize the form smoothly
        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            if (expanding)
            {
                // Expand the form width
                if (this.Width < targetWidth)
                {
                    this.Width += resizeStep;
                    if (this.Width > targetWidth) this.Width = targetWidth; // Stop at target width
                }
                else
                {
                    resizeTimer.Stop(); // Stop resizing when target width is reached
                }
            }
            else
            {
                // Collapse the form width
                if (this.Width > targetWidth)
                {
                    this.Width -= resizeStep;
                    if (this.Width < targetWidth) this.Width = targetWidth; // Stop at target width
                }
                else
                {
                    resizeTimer.Stop(); // Stop resizing when target width is reached
                }
            }
        }

        // Method to save the output to a file
        private void SaveOutputToFile(string output)
        {
            try
            {
                string filePath = Path.Combine(Application.StartupPath, "output.txt");

                // Write the output to the file
                File.WriteAllText(filePath, output);
            }
            catch (Exception ex)
            {
                // Show error if saving fails
                MessageBox.Show($"Failed to save output to file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event handler to open and save the output to a file when clicked
        private void labelOpenOutput_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(storedOutput))
            {
                // Save the stored output to a file
                SaveOutputToFile(storedOutput);

                // Open the saved file in Notepad
                string filePath = Path.Combine(Application.StartupPath, "output.txt");
                Process.Start("notepad.exe", filePath);
            }
            else
            {
                MessageBox.Show("No output to save. Please run a command first.", "No Output", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
