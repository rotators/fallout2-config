using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace fo2_config
{
    public class ddraw
    {
        static public string Version;
        static public string Path;

        static public List<iniOption> options = new List<iniOption>();
        static public List<IRender> renderables = new List<IRender>();

        static public Action redrawCallback;

        public static void Init()
        {
            AddBool("Main", "UseCommandLine", "Use Commandline", "Enable if you want to use command line args to tell sfall to use another ini file.");
            AddInt("Sound", "NumSoundBuffers", "Number of sound buffers", "Sets the number of allowed simultaneous sound effects. Set to 0 to leave the default unchanged (i.e. 4)");
            AddBool("Sound", "AllowSoundForFloats", "Allow sound files for combat float messages", "");
            var dsound = AddIntEnum("Sound", "AllowDShowSound", "DirectShow Sound", "This does not effect the play_sfall_sound and stop_sfall_sound script functions", new Dictionary<string, int>
            {
                { "Disabled", 0 },
                { "Automatically search for alternative formats", 1 },
                { "Play alternative music files", 2 },
            });
            dsound.valueDescription = @"1:Automatically search for alternative formats (mp3/wma/wav) when Fallout tries to play an ACM:" +
                                      @"2:Play alternative music files even if original ACM files are not present in the music folder:";

            var dir = AddIntEnum("Sound", "OverrideMusicDir", "Override Music Directory", "", new Dictionary<string, int>
            {
                { "Disabled", 0 },
                { "Override the music path used by default", 1 },
                { "Overwrite all occurances of the music path", 2 },
            });
            dir.valueDescription = @"1:Override the music path used by default (i.e. data\sound\music\) if not present in the cfg:" +
                                    "2:Overwrite all occurances of the music path:";


            AddIntEnum("Graphics", "Mode", "Graphics mode", "A DX9 mode is required for any graphics related script extender functions to work (i.e. fullscreen shaders).", new Dictionary<string, int>
            {
                { "8-bit Fullscreen", 0 },
                { "DX9 Fullscreen", 4 },
                { "DX9 Windowed", 5 },
            });
            AddIntEnum("Graphics", "GPUBlt", "Blit mode", "GPU is faster, but requires v2.0 pixel shader support.", new Dictionary<string, int>
            {
                { "Pick automatically", 0 },
                { "Palette conversion on the GPU", 1 },
                { "Palette conversion on the CPU", 2 },
            });

            var r = AddResolution("Graphics", "Resolution", "GraphicsWidth,GraphicsHeight",
                "If using a DX9 mode, this changes the resolution.\nThe graphics are simply stretched to fit the new window; this does _not_ let you see more of the map\nIf set to 0, use Fallout's native resolution");
            r.displayCondition = "Graphics::Mode > 0";

            var h = AddBool("Graphics", "Use32BitHeadGraphics", "Allow using 32-bit textures for talking heads",
                "The texture files should be placed in art\\heads\\<name of the talking head FRM file>\\ (w/o extension)\n" +
                "The files in the folder should be numbered according to the number of frames in the talking head FRM file (0.png, 1.png, etc.)\n" +
                "See the text file in the modders pack for a detailed description\n" +
                "Requires DX9 graphics mode and v2.0 pixel shader support.");
            h.displayCondition = "Graphics::Mode > 0";

            AddInt("Graphics", "FadeMultiplier", "Fade multiplier", "Fade effect time percentage modifier\nDefault is 100. Decrease/increase this value to speed up/slow down fade effects");
        }

        static public iniOption FindIniOption(string unifiedName)
        {
            var spl = unifiedName.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
            return FindIniOption(spl[0], spl[1]);
        }

        static public iniOption FindIniOption(string category, string name)
            => ddraw.options.Where(z => z.category == category && z.name == name).SingleOrDefault();

        static public int GetInt(string category, string name)
        {
            var o = FindIniOption(category, name);
            if (o == null) return -1;
            return int.Parse(o.inidata);
        }

        static iniOption BaseOption(string category, string name, optionType type, string label, string description)
        {
            return new iniOption()
            {
                category = category,
                name = name,
                type = type,
                label = label,
                description = description,
                inidata = "",
                enabled = true,
                found=false
            };
        }

        static resolutionOption AddResolution(string category, string name, string iniOptions, string description)
        {
            var opt = new resolutionOption()
            {
                category = category,
                name = name,
                description = description,
            };
            return opt;
        }

        static void AddBoolString(string category, string name, string label, string description)
        {
            var opt = BaseOption(category, name, optionType.booleanString, label, description);
            AddOption(opt);
        }

        static void AddOption(iniOption opt)
        {
            options.Add(opt);
            renderables.Add(opt);
        }

        static iniOption AddBool(string category, string name, string label, string description)
        {
            var opt = BaseOption(category, name, optionType.boolean, label, description);
            AddOption(opt);
            return opt;
        }

        static void AddInt(string category, string name, string label, string description)
        {
            var opt = BaseOption(category, name, optionType.integer, label, description);
            AddOption(opt);
        }

        static iniOption AddIntEnum(string category, string name, string label, string description, Dictionary<string, int> enums)
        {
            var opt = BaseOption(category, name, optionType.intEnum, label, description);
            opt.enums = enums;
            AddOption(opt);
            return opt;
        }

        static public void Parse(string path)
        {
            Path = path;
            string category="";
            bool enabled = false;

            foreach(var line in File.ReadAllLines(path))
            {
                if (line.Length == 0)
                    continue;

                if (line[0] == ';' && line[1] == 'v')
                    Version = line.Substring(2);
                if (line[0] == '[')
                    category = line.Substring(1, line.Length - 2);

                if (!line.Contains("="))
                    continue;

                enabled = true;
                string keyValue = line;

                // commented option
                if(line[0]==';')
                {
                    enabled = false;
                    keyValue = line.Substring(1);
                }

                var spl = keyValue.Split('=');
                var key = spl[0];
                var val = spl[1];

                var found = options.SingleOrDefault(x => x.category == category && x.name == key);
                if(found != null)
                {
                    found.inidata = val;
                    found.enabled = enabled;
                    found.found = true;
                }

            }
        }

    }

    public enum optionType
    {
        boolean,
        booleanString, // string with a checkbox to enable, if not enabled it means it's commented out.
        intEnum,
        integer,
        path,
    }

    public interface IRender
    {
        int Render(Control parent, int y);
    };

    public class resolutionOption : IRender
    {
        public string name;
        public string description;
        public string category;
        public string iniOptions;
        public string displayCondition;

        public int Render(Control parent, int y)
        {
            if (!string.IsNullOrEmpty(displayCondition))
            {
                if (!DisplayConditionParse.IsConditionTrue(displayCondition))
                    return y;
            }

            var spl = iniOptions.Split(',');
            var wOpt = ddraw.FindIniOption(category, spl[0]);
            var hOpt = ddraw.FindIniOption(category, spl[1]);

            var width = int.Parse(wOpt.inidata);
            var height = int.Parse(hOpt.inidata);

            var grp = new GroupBox();
            grp.Text = name;
            grp.Parent = parent;
            grp.Location = new Point(10, y);
            grp.AutoSize = true;

            int grpY = 25;
            int x = 10;

            var num1 = new NumericUpDown();
            num1.Width = 50;
            num1.Location = new Point(x, grpY);
            num1.Value = width;
            num1.Parent = grp;
            num1.ValueChanged += (object sender, EventArgs e) => wOpt.inidata = num1.Value.ToString();

            x += 40;

            var label = new Label();
            label.Text = " x ";

            var num2 = new NumericUpDown();
            num2.Width = 50;
            num2.Location = new Point(x, grpY);
            num2.Value = width;
            num2.Parent = grp;
            num2.ValueChanged += (object sender, EventArgs e) => wOpt.inidata = num1.Value.ToString();

            x += 40;

            if (!string.IsNullOrEmpty(description))
            {
                grpY += 25;
                var dsc = new Label();
                dsc.AutoSize = true;
                dsc.Text = description;
                dsc.Location = new Point(x, grpY);
                dsc.Parent = grp;
            }

            grp.Height = grpY;

            y += grp.Height - 20;

            return y;
        }
    }

    public class DisplayConditionParse
    {
        public string category;
        public string name;
        public string op;
        public string val;

        public DisplayConditionParse(string displayCondition)
        {
            var reg = new Regex("(.+)::(.+)\\s+([<>!=])+\\s+(\\d+)");
            var match = reg.Match(displayCondition);
            if (match.Success)
            {
                category = match.Groups[1].Value;
                name = match.Groups[2].Value;
                op = match.Groups[3].Value;
                val = match.Groups[4].Value;
            }
        }

        public static bool IsConditionTrue(string displayCondition)
        {
            var parsed = new DisplayConditionParse(displayCondition);
            var opt = ddraw.FindIniOption(parsed.category, parsed.name);
            if (opt != null)
            {
                if (parsed.op == ">" && int.Parse(opt.inidata) <= int.Parse(parsed.val))
                    return false;
                if (parsed.op == "<" && int.Parse(opt.inidata) >= int.Parse(parsed.val))
                    return false;
                if (parsed.op == "==" && int.Parse(opt.inidata) != int.Parse(parsed.val))
                    return false;
                if (parsed.op == "!=" && int.Parse(opt.inidata) == int.Parse(parsed.val))
                    return false;
            }
            return true;
        }
    }

    public class iniOption : IRender
    {
        public string category;
        public string label;
        public string name;
        public string description;
        public optionType type;
        public bool found;
        public bool enabled;
        public bool dontRender; // if used in a multifield option.
        public int min; // only for int
        public int max; // only for int
        public string inidata;
        public string displayCondition;
        public string valueDescription;
        public Dictionary<string, int> enums;
        
        public int Render(Control parent, int y)
        {
            if(!string.IsNullOrEmpty(displayCondition))
            {
                if (!DisplayConditionParse.IsConditionTrue(displayCondition))
                    return y;
            }

            var x = 10;
            if (type == optionType.boolean)
            {
                var chk = new CheckBox();
                chk.AutoSize = true;
                chk.Location = new Point(x, y);
                chk.Checked = inidata == "1";
                chk.Parent = parent;
                chk.CheckedChanged += (object sender, EventArgs e) => inidata = chk.Checked ? "1" : "0";
                x += 15;
                var lbl2 = new Label();
                lbl2.Text = label;
                lbl2.Parent = parent;
                lbl2.Location = new Point(x, y);
                lbl2.AutoSize = true;
                lbl2.MouseClick += (_, __) =>
                {
                    chk.Checked = !chk.Checked;
                    inidata = chk.Checked ? "1" : "0";
                };

                if (!string.IsNullOrEmpty(description))
                {
                    y += 18;
                    var label = new Label();
                    label.AutoSize = true;
                    label.Text = description;
                    label.Location = new Point(x, y);
                    label.Parent = parent;
                    y += label.Height;
                }

                
            }
            if (type == optionType.integer)
            {
                /*var lbl2 = new Label();
                lbl2.Text = label;
                lbl2.Parent = parent;
                lbl2.Location = new Point(x, y);
                lbl2.AutoSize = true;
                */
                var grp = new GroupBox();
                grp.Text = label;
                grp.Parent = parent;
                grp.Location = new Point(x, y);
                grp.AutoSize = true;

                int grpY = 25;

                var num = new NumericUpDown();
                num.Width = 35;
                num.Location = new Point(x, grpY);
                num.Value = int.Parse(inidata);
                num.Parent = grp;
                num.ValueChanged += (object sender, EventArgs e) => inidata = num.Value.ToString();

                x = 10;

                if (!string.IsNullOrEmpty(description))
                {
                    grpY += 25;
                    var label = new Label();
                    label.AutoSize = true;
                    label.Text = description;
                    label.Location = new Point(x, grpY);
                    label.Parent = grp;
                }

                grp.Height = grpY;
                y += grp.Height - 20;
            }

            if (type == optionType.intEnum)
            {
                
                var grp = new GroupBox();
                grp.Text = label;
                grp.Parent = parent;
                grp.Location = new Point(x, y);
                grp.AutoSize = true;

                int grpY = 25;

                var cmb = new ComboBox();
                cmb.Location = new Point(x, grpY - 2);
                cmb.Parent = grp;
                cmb.DropDownStyle = ComboBoxStyle.DropDownList;

                int i = 0;
                int selectedIdx = 0;
                int maxWidth = 0;
                foreach (var e in enums)
                {
                    cmb.Items.Add(e.Key);
                    var temp = TextRenderer.MeasureText(e.Key, cmb.Font).Width + 20;
                    if (temp > maxWidth)
                        maxWidth = temp;

                    if (e.Value.ToString() == inidata)
                        selectedIdx = i;
                    
                    i++;
                }
                cmb.SelectedIndex = selectedIdx;
                cmb.Width = maxWidth;
                cmb.SelectedIndexChanged += (object sender, EventArgs ee) =>
                {
                    if (cmb.SelectedIndex == -1)
                        return;
                    inidata = enums[cmb.Text].ToString();
                    ddraw.redrawCallback();
                };

                x = 10;

                string renderDescription = "";
                int val = int.Parse(inidata);

                if (!string.IsNullOrEmpty(valueDescription))
                {
                    int idxValueDesc = valueDescription.IndexOf(val + ":");
                    if (idxValueDesc != -1)
                    {
                        idxValueDesc += 2;
                        int stopIdx = valueDescription.IndexOf(':', idxValueDesc);
                        int len = stopIdx - idxValueDesc;
                        renderDescription += valueDescription.Substring(idxValueDesc, len) + "\n\n";
                    }
                }

                if (!string.IsNullOrEmpty(description))
                    renderDescription += description;

                if (!string.IsNullOrEmpty(renderDescription))
                {
                    grpY += 25;
                    var label = new Label();
                    label.AutoSize = true;
                    label.Text = renderDescription;
                    label.Location = new Point(x, grpY);
                    label.Parent = grp;
                }

                grp.Height = grpY;

                y += grp.Height - 20;
            }

            


            return y;
        }
    }
}
