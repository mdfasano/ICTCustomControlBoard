using System;
using System.Drawing;
using System.Text.Json;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using IctCustomControlBoard;

namespace IctControlPanel
{
    public class BoardForm : Form
    {
        private readonly Dictionary<string, Control> _lights;
        private List<string> _inputMapping = [];
        private List<string> _outputMapping = [];

        // This stores the state of ALL output bits (0-63)
        private ulong _outputMasterState = 0;

        private BoardManager _boardManager;
        public BoardForm()
        {
            // 1. Makes the window take up the whole screen
            this.WindowState = FormWindowState.Maximized;

            this.Load += new EventHandler(BoardForm_Load);

            // 2. Removes the standard window borders
            //this.FormBorderStyle = FormBorderStyle.None;
            Text = "ICT Control Panel";
            AutoScroll = true;
            BackColor = Color.WhiteSmoke;

            // initialize the lights collection
            _lights = [];
            LoadMapping();

            var mainPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(5)
            };
            Controls.Add(mainPanel);

            // Device 3
            var inputsContainer = CreateCategoryGroup("Readback Inputs (Device3 and Device4)");

            inputsContainer.Controls.Add(BuildDeviceSection(
                "inputs",
                [
                    // the order displayed here is not logical, the grouping is for display clarity
                    ["KL1", "KL4", "KL5", "KL6", "KL7", "KL8"],
                    ["KT1", "KT2", "KT3", "KT4", "KT5", "KT6", "KT7", "KT8", "KT9"],
                    ["KRFT", "KCFT1", "KCFT2", "KINJ1"],
                    ["KRN1", "KRN2", "KRN3", "KRN4", "KRN5", "KRN6", "KRN7", "KRN8"],
                    ["KRG1", "KRG2", "KRG3", "KRG4", "KRG5", "KRG6", "KRG7", "KRG8"],
                    ["AI1", "AI2"]
                ]));
            mainPanel.Controls.Add(inputsContainer); // attach inputgroup to main panel


            // --- OUTPUTS COLUMN (Device 1 & 2) ---
            var outputsContainer = CreateCategoryGroup("Outputs (Device1 and Device2)");

            // Example data for Device 1 & 2 - Replace labels with your actual mapping
            outputsContainer.Controls.Add(BuildDeviceSection("Outputs", [
                ["KT5", "KT6", "KT7", "KT8", "KT9", "KT1", "KT2", "KT3", "KT4"],
                ["KRG1", "KRG2", "KRG3", "KRG4", "KRG5"],
                ["KL1", "KL4KL5", "KL7KL8", "KL6KL7KL8_EN"],
                ["KRFT", "KCFT1", "KCFT2", "KINJ1"]
            ]));
            mainPanel.Controls.Add(outputsContainer); // attach output group to main panel

            // add a nav panel
            var navPanel = CreateButtonPanel();
            Controls.Add(navPanel);
            navPanel.BringToFront(); // Ensure it stays above the scrolling content
        }


        private void BoardForm_Load(object sender, EventArgs e)
        {
            try
            {
                _boardManager = new BoardManager();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Config Error: {ex.Message}\n\n " +
                    $"check that app.config matches internal device names in NI-Max \n\n " +
                    $"Also make sure all 4 devices are connected");
                this.Close();
            }
        }
        // creates a box in the forms gui to organize the displayed relay data into neat rows
        private GroupBox BuildDeviceSection(string title, string[][] rows)
        {
            var group = new GroupBox
            {
                Text = title,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(5),
                Margin = new Padding(5),
            };

            var layout = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                //Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = rows.Length,
                Padding = new Padding(5),
            };
            group.Controls.Add(layout);

            bool isInteractive = (title == "Outputs");

            foreach (var row in rows)
            {
                var rowPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true,
                    Margin = new Padding(0, 2, 0, 2) // was 15
                };

                foreach (var label in row)
                {
                    rowPanel.Controls.Add(BuildLightWithLabel(label, isInteractive));
                }

                layout.Controls.Add(rowPanel);
            }

            return group;
        }
         
        // creates a layout group
        private static FlowLayoutPanel CreateCategoryGroup(string title)
        {
            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            // --- FIX: Create a label using the 'title' argument ---
            var headerLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10) // Space between title and first device
            };

            // Add the label to the panel *before* returning it
            panel.Controls.Add(headerLabel);
            return panel;
        }

        // sets up the label/light pair using the passed string
        // returns an object holding the display data
        private TableLayoutPanel BuildLightWithLabel(string labelText, bool isInteractive = false)
        {
            // 1. Container as a TableLayoutPanel for automatic centering
            var container = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(10, 5, 10, 5)
            };
            // Make the single column center its contents
            container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            // 2. The Label - now with AutoSize to handle long text
            var label = new Label
            {
                Text = labelText,
                TextAlign = ContentAlignment.BottomCenter,
                AutoSize = true,
                Anchor = AnchorStyles.None, // Anchors to 'None' inside a TableCell to center it
                MaximumSize = new Size(150, 0), // Allows wrapping if text is massive
                Font = new Font("Segoe UI", 8f, FontStyle.Regular),
                Margin = new Padding(0, 0, 0, 2)
            };

            // 3. The Light Panel
            var light = new Button
            {
                Width = 18,
                Height = 18,
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.None, // Anchors to 'None' to stay centered
                Margin = new Padding(0, 2, 0, 0),
                Cursor = isInteractive ? Cursors.Hand : Cursors.Default,
                Enabled = true
            };
            light.FlatAppearance.BorderSize = 1;

            if (isInteractive)
            {
                light.Cursor = Cursors.Hand;
                // POINT TO THE CLEAN FUNCTION
                light.Click += HandleOutputClick;
            }

            // 4. Add to the table grid
            container.Controls.Add(label, 0, 0); // Row 0
            container.Controls.Add(light, 0, 1); // Row 1

            _lights.TryAdd(labelText, light);

            return container;
        }

        private FlowLayoutPanel CreateButtonPanel()
        {
            var buttonPanel = new FlowLayoutPanel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right, // Anchor to Top-Right corner
                Size = new Size(200, 45), // 2 buttons
                Location = new Point(this.ClientSize.Width - 215, 10), // Position changes relative to form size
                FlowDirection = FlowDirection.RightToLeft, // Aligns buttons to the right
                Padding = new Padding(0),
                BackColor = Color.Gainsboro
            };

            // Define buttons
            Button btnFetch = new() { Text = "get readback data", Width = 80, Height = 30, BackColor = Color.LimeGreen, Cursor = Cursors.Hand };
            Button btnReset = new() { Text = "clear", Width = 80, Height = 30, Cursor = Cursors.Hand };

            // Events
            btnReset.Click += (s, e) => BulkSetLights(null);
            btnFetch.Click += (s, e) => {
                // Call your logic here
                ulong latestData = GetDeviceData();
                UpdateInputLightsFromData(latestData);
            };

            // Add to panel
            buttonPanel.Controls.Add(btnReset);
            buttonPanel.Controls.Add(btnFetch);

            return buttonPanel;
        }

        // turns 'lights' on (green) or off (red)
        // sets 'light' color to grey if no second parameter is passed
        public void SetLightStatus(string lightName, bool? isOn)
        {
            if (_lights.ContainsKey(lightName))
            {
                if (isOn == true)
                    _lights[lightName].BackColor = Color.LimeGreen;
                else if (isOn == false)
                    _lights[lightName].BackColor = Color.Red;
                else
                    _lights[lightName].BackColor = Color.LightGray;
            }
        }

        private void BulkSetLights(bool? status)
        {
            foreach (var light in _lights.Values)
            {
                if (status == true)
                    light.BackColor = Color.LimeGreen;
                else if (status == false)
                    light.BackColor = Color.Red;
                else
                    light.BackColor = Color.LightGray;
            }
        }

        public void UpdateInputLightsFromData(ulong packedData)
        {
            // Iterate through the mapping
            for (int i = 0; i < _inputMapping.Count; i++)
            {
                string label = _inputMapping[i];

                // If the label is "EMPTY" or null, skip the dictionary lookup
                if (string.IsNullOrEmpty(label) || label.StartsWith("EMPTY")) continue;

                // Extract the bit at position 'i'
                bool isOn = (packedData & (1UL << i)) != 0;

                SetLightStatus(label, isOn);
            }
        }

        // loads the mapping of bits -> labels from mapping.json
        private void LoadMapping()
        {
            string jsonString = string.Empty;
            string externalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mapping.json");
            try
            {
                if (File.Exists(externalPath))
                {
                    jsonString = File.ReadAllText(externalPath);

                }
                else
                {
                    // 2. Fallback: Load from Embedded Resource if local file does not exist
                    var assembly = Assembly.GetExecutingAssembly();
                    // Ensure this matches your [Namespace].[Filename]
                    string resourceName = "IctControlPanel.mapping.json";

                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream == null) throw new Exception("Embedded resource not found.");
                        using StreamReader reader = new(stream);
                        jsonString = reader.ReadToEnd();
                    }

                    // Export the embedded version to a file if it is gone
                    // This creates a template for the user to edit!
                    File.WriteAllText(externalPath, jsonString);
                }

                // parse json
                var data = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonString);

                _inputMapping = data?.ContainsKey("InputMapping") == true ? data["InputMapping"] : [];
                _outputMapping = data?.ContainsKey("OutputMapping") == true ? data["OutputMapping"] : [];
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading config: {ex.Message}");

            }
        }

        // pulls data by calling boardmanager.getbits
        private ulong GetDeviceData()
        {
            try
            {
                // This is where you call your library code
                return _boardManager.GetBits();

                // FOR TESTING: Returning a dummy value (Bit 0 and Bit 7 on)
                //return 0xffffffffff;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Device Read Error: {ex.Message}");
                return 0;
            }
        }

        // executes when a 'light' in the output section is clicked
        // calls boardmanager.setbits 
        private void HandleOutputClick(object sender, EventArgs e)
        {
            // 1. Identify which button was clicked
            if (sender is Button btn && btn.Parent is TableLayoutPanel container)
            {
                // Find the label associated with this button to get the hardware name
                // The label is in Row 0, the button is in Row 1
                var label = container.GetControlFromPosition(0, 0) as Label;
                if (label == null) return;

                // 1. Find where this button lives in the 64-bit sequence
                int globalIndex = _outputMapping.IndexOf(label.Text);
                if (globalIndex == -1) return;

                // Note: If Device 2 starts at index 24, globalIndex is already correct.
                // We use 1UL to ensure we are doing 64-bit math.
                ulong bitMask = 1UL << globalIndex;

                try
                {
                    // 2. Determine if we are turning it ON or OFF
                    bool isTurningOn = btn.BackColor != Color.LimeGreen;

                    if (isTurningOn)
                    {
                        // Bitwise OR sets the bit to 1
                        _outputMasterState |= bitMask;
                    }
                    else
                    {
                        // Bitwise AND with a NOT mask sets the bit to 0
                        _outputMasterState &= ~bitMask;
                    }

                    // 3. Send the entire updated 64-bit state to the board
                    _boardManager.SetBits(_outputMasterState);

                    // 4. Update UI
                    btn.BackColor = isTurningOn ? Color.LimeGreen : Color.LightGray;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hardware Error: {ex.Message}");
                }
            }
        }
    }
}
