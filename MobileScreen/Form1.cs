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
        public Form1()
        {
            InitializeComponent();
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

        private void button3_Click(object sender, EventArgs e)
        {
            string adbCommand = txtCommand.Text.Trim(); // Get the command entered in the textbox

            // Validate if the textbox is empty
            if (string.IsNullOrEmpty(adbCommand))
            {
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
                // Run the adb command entered by the user
                string commandOutput = RunCommand(adbCommand); // Use your existing RunCommandWait method
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while executing the command:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}