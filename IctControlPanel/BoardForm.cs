using System;
using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;
using IctCustomControlBoard;

namespace IctControlPanel
{
    public class BoardForm : Form
    {
        private readonly Dictionary<string, Panel> _lights;
        private List<string> _bitMapping;
        BoardManager board;
        public BoardForm()
        {
            Text = "ICT Control Panel";
            Width = 1100;
            Height = 900;
            AutoScroll = true;
            BackColor = Color.WhiteSmoke;

            // initialize the lights collection
            _lights = [];
            _bitMapping = LoadMapping();
            board = new BoardManager();

            var mainPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(5)
            };
            Controls.Add(mainPanel);

            // Device 3
            mainPanel.Controls.Add(BuildDeviceSection(
                "Device 3",
                [
                    ["KL1", "KL4", "KL5", "KL6", "KL7", "KL8"],
                    ["KT1", "KT2", "KT3", "KT4", "KT5", "KT6", "KT7", "KT8", "KT9"],
                    ["KRFT"],
                    ["KCFT1", "KCFT2"],
                    ["KRN1", "KRN2", "KRN3", "KRN4", "KRN5", "KRN6"]
                ]));

            // Device 4
            mainPanel.Controls.Add(BuildDeviceSection(
                "Device 4",
                [
                    ["AI1", "AI2"],
                    ["KRN7", "KRN8", "KRG1", "KRG2", "KRG3", "KRG4", "KRG5", "KRG6", "KRG7", "KRG8"],
                    ["KINJ1"]
                ]));

            // add a nav panel
            var navPanel = CreateButtonPanel();
            Controls.Add(navPanel);
            navPanel.BringToFront(); // Ensure it stays above the scrolling content
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
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = rows.Length,
                Padding = new Padding(5),
            };
            group.Controls.Add(layout);

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
                    rowPanel.Controls.Add(BuildLightWithLabel(label));
                }

                layout.Controls.Add(rowPanel);
            }

            return group;
        }

        // sets up the label/light pair using the passed string
        // returns an object holding the display data
        private Panel BuildLightWithLabel(string labelText)
        {
            var container = new Panel
            {
                Width = 70,
                Height = 60,
                Margin = new Padding(5),
            };

            var label = new Label
            {
                Text = labelText,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Segoe UI", 8, FontStyle.Regular)
            };

            var light = new Panel
            {
                Width = 25,
                Height = 25,
                BackColor = Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle,
                Top = 30,
                Left = 22,
            };

            // Add to our dictionary so we can find it later
            _lights.TryAdd(labelText, light);

            container.Controls.Add(light);
            container.Controls.Add(label);
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
            Button btnFetch = new() { Text = "refresh data", Width = 80, Height = 30, BackColor = Color.LimeGreen, Cursor = Cursors.Hand };
            Button btnReset = new() { Text = "Reset", Width = 80, Height = 30, Cursor = Cursors.Hand };

            // Events
            btnReset.Click += (s, e) => BulkSetLights(null);
            btnFetch.Click += (s, e) => {
                // Call your logic here
                ulong latestData = GetDeviceData();
                UpdateLightsFromData(latestData);
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

        public void UpdateLightsFromData(ulong packedData)
        {
            // Iterate through the mapping
            for (int i = 0; i < _bitMapping.Count; i++)
            {
                string label = _bitMapping[i];

                // If the label is "EMPTY" or null, skip the dictionary lookup
                if (string.IsNullOrEmpty(label) || label.StartsWith("EMPTY")) continue;

                // Extract the bit at position 'i'
                bool isOn = (packedData & (1UL << i)) != 0;

                SetLightStatus(label, isOn);
            }
        }

        // loads the mapping of bits -> labels from mapping.json
        private static List<string> LoadMapping()
        {
            try
            {
                string jsonString = File.ReadAllText("mapping.json"); 
                var data = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonString);

                return data != null && data.ContainsKey("BitOrder")
               ? data["BitOrder"]
               : new List<string>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading config: {ex.Message}");
                return new List<string>();
            }
        }

        // pulls data by calling boardmanager.getbits
        private ulong GetDeviceData()
        {
            try
            {
                // This is where you call your library code
                return board.GetBits();

                // FOR TESTING: Returning a dummy value (Bit 0 and Bit 7 on)
                //return 0xffffffffff;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Device Read Error: {ex.Message}");
                return 0;
            }
        }
    }
}
