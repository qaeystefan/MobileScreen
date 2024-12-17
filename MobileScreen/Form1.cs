using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MobileScreen
{
    public partial class Form1 : Form
    {

        // Store initial form width and expanded width
        private int initialFormWidth;
        private int expandedFormWidth;

        private Timer resizeTimer;       // Timer for smooth resizing
        private int targetWidth;         // The target width for the form
        private int resizeStep = 28;     // Step size for each tick (adjust for speed)
        private bool expanding;          // Flag to indicate expand or collapse


        public Form1()
        {
            InitializeComponent();
            // ToolTip for your label
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(label9, "Click to see ADB Commands list");

            // Define initial and expanded widths
            initialFormWidth = 234;  // Adjust to match the width for "1"
            expandedFormWidth = 962; // Adjust to include "1 | 2"
            this.Width = initialFormWidth; // Start with initial width
            txtOutput.Visible = false;

            // Initialize the Timer for smooth transition
            resizeTimer = new Timer();
            resizeTimer.Interval = 1; // Adjust for speed (lower = faster)
            resizeTimer.Tick += ResizeTimer_Tick;

        }
        private string RunCommand(string command)
        {
            try
            {
                // Initialize the process to run the command
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c " + command, // "/c" runs the command and closes the cmd
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true // Run without showing the cmd window
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
                return $"Error running command: {ex.Message}";
            }
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            string ip = txtIP.Text.Trim();
            string port = txtPort.Text.Trim();

            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port))
            {
                MessageBox.Show("Please provide both IP and Port.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string adbCommand = $"adb connect {ip}:{port}";
            string scrcpyCommand = "scrcpy.exe";

            try
            {
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

                // Run the ADB command in the background
                string adbOutput = await Task.Run(() => RunCommand(adbCommand));
                if (!adbOutput.Contains("connected to"))
                {
                    MessageBox.Show($"ADB connection failed:\n{adbOutput}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Run scrcpy command and capture its output in the background
                string scrcpyOutput = await Task.Run(() => RunCommand(scrcpyCommand));

                if (scrcpyOutput.Contains("Could not find any ADB device") || scrcpyOutput.Contains("Server connection failed"))
                {
                    MessageBox.Show($"scrcpy failed:\n{scrcpyOutput}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Close(); // Close the form after successful execution
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            string ip = txtIP2.Text.Trim();
            string port = txtPort2.Text.Trim();
            string pairingCode = txtPairCode.Text.Trim(); // Get the pairing code from the textbox

            // Validate IP and Port inputs
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

                // Run the first adb pair command and capture its output
                string adbPairOutput = RunCommand(adbPairCommand); // This will keep the CMD open

                // Check if the output asks for a pairing code
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
                MessageBox.Show($"An error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            string adbCommand = txtCommand.Text.Trim();

            if (string.IsNullOrEmpty(adbCommand))
            {
                MessageBox.Show("Please enter an adb command.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists("adb.exe"))
            {
                MessageBox.Show("adb.exe not found in the application directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string commandOutput = await Task.Run(() => RunCommand(adbCommand));

                if (!string.IsNullOrEmpty(commandOutput))
                {
                    txtOutput.Text = commandOutput;

                    // Show output TextBox and expand form width
                    //txtOutput.Visible = true;
                    //txtOutput.Width = 300; // Adjust width for expansion
                    //this.Width = expandedFormWidth; // Expand form width
                }
                else
                {
                    MessageBox.Show("No output returned for the command.", "No Output", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while executing the command:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void label9_Click(object sender, EventArgs e)
        {
            string filePath = Path.Combine(Application.StartupPath, "Commands.txt"); // File in the same directory as the app

            if (File.Exists(filePath))
            {
                Process.Start(filePath); // Open the file with default program (Notepad)
            }
            else
            {
                MessageBox.Show("The Commands.txt file was not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void topicsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Code to display Help Topics
            MessageBox.Show("Follow the steps below to get started quickly.\n\n" +
                            "- Connecting to an Android Device\n" +
                            "Step 1: Enter the IP Address and Port of your Android device in the provided fields.\n" +
                            "Step 2: Click Connect.\n" +
                            "The app will attempt to establish a connection with the Android device using the ADB connect command.\n" +
                            "If successful, the Android device screen will be mirrored on your PC using scrcpy.\n\n" +
                            "- Pairing with an Android Device\n" +
                            "If you're connecting to a device for the first time, you may need to pair it.\n" +
                            "Step 1: Enter the IP Address, Port, and Pairing Code provided by the device.\n" +
                            "Step 2: Click Pair.\n" +
                            "The app will send the pairing code to the Android device, establishing a secure connection.\n" +
                            "Once paired successfully, the device will be ready for control and screen mirroring.\n\n" +
                            "- Running ADB Commands\n" +
                            "Step 1: Enter any ADB command in the Command text box.\n" +
                            "Step 2: Click Run Command.\n" +
                            "The app will execute the command, and any output or errors will be displayed in the app.\n" +
                            "You will see the command's output in the output box, which can help troubleshoot or confirm command execution.\n\n" +
                            "- Opening ADB Command List\n" +
                            "Click on the Commands label (located in the app’s interface) to view a list of available ADB commands.\n" +
                            "This file is accessible from the Commands.txt file located in the app's directory.\n\n" +
                            "- Troubleshooting\n" +
                            "Ensure that adb.exe and scrcpy.exe are located in the same directory as the application.\n" +
                            "If the connection fails, double-check the IP Address and Port you entered.\n" +
                            "If scrcpy does not start, confirm that your Android device is properly connected and ready to mirror.\n\n" +
                            "- Help and Support\n" +
                            "For any additional questions or concerns, please feel free to send a direct message via Teams.",
                            "Help Topics", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Code to display About App information
            MessageBox.Show("This app serves as a GUI wrapper for ADB and scrcpy tool, simplifying the process of connecting, pairing, and running commands for Android devices. It’s particularly useful for:\n\n" +
                            "QA and Developers testing Android devices over a network.\n" +
                            "Users who want a quick way to mirror or control their Android screens on a PC.\n\n" +
                            "Version 1.0\nCreated by Stefan",
                            "About", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void labelToggleOutput_Click(object sender, EventArgs e)
        {
            // Toggle the expanding direction based on current width
            if (this.Width == expandedFormWidth)
            {
                targetWidth = initialFormWidth; // Collapse to initial width
                expanding = false;
                //txtOutput.Visible = false; // Hide the TextBox when collapsing
            }
            else
            {
                targetWidth = expandedFormWidth; // Expand to full width
                expanding = true;
                txtOutput.Visible = true; // Show the TextBox when expanding
            }

            // Start the Timer for the smooth resizing effect
            resizeTimer.Start();
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            if (expanding)
            {
                // Expand the form width
                if (this.Width < targetWidth)
                {
                    this.Width += resizeStep;
                    if (this.Width > targetWidth) this.Width = targetWidth; // Stop at target
                }
                else
                {
                    resizeTimer.Stop(); // Stop Timer when done
                }
            }
            else
            {
                // Collapse the form width
                if (this.Width > targetWidth)
                {
                    this.Width -= resizeStep;
                    if (this.Width < targetWidth) this.Width = targetWidth; // Stop at target
                }
                else
                {
                    resizeTimer.Stop(); // Stop Timer when done
                }
            }
        }

    }
}