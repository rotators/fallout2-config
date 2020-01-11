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
    public class f2Res : ini
    {
        public f2Res()
        {
            AddBool("MAIN", "UAC_AWARE", "UAC Aware", "%C_ALL");
            AddIntEnum("MAIN", "GRAPHICS_MODE", "Graphics Mode", "", new Dictionary<string, int>()
            {
                { "Basic mode (Required for sfall graphic modes)", 0 },
                { "Direct Draw 7 mode", 1 },
                { "DX9 mode", 2 },
            });
            AddBool("MAIN", "SCALE_2X", "Scale the game x2", "%C_2");
            AddBool("MAIN", "WINDOWED", "Enable windowed mode", "");
            // We use this in the resolution control below
            AddInt("MAIN", "SCR_WIDTH", "", "").dontRender = true;
            AddInt("MAIN", "SCR_HEIGHT", "", "").dontRender = true;
            var r = AddResolution("MAIN", "Fullscreen resolution", "SCR_WIDTH,SCR_HEIGHT", null);

            AddIntEnum("MAIN", "COLOUR_BITS", "Fullscreen colours", "", new Dictionary<string, int>()
            {
                { "8-bit colour output (original)", 8 },
                { "16-bit colour output", 16 },
                { "32-bit colour output", 32 },
            });

            AddInt("MAIN", "REFRESH_RATE", "Fullscreen refresh rate", "%C_ALL");
            AddBool("EFFECTS", "IS_GRAY_SCALE", "Play the game in black and white. (Pixote's zany idea)", null);
        }
    }

    public class DDRAW : ini
    {
        public void InitSpeed()
        {
            AddBool("Speed", "Enable", "Enable speed options", "");

            var speedEnabled = "Speed::Enable > 0";

            AddLabel("Speed", "The speeds corresponding to each slot in percent. (i.e. 100 is normal speed)");
            var multi = AddIntRange("Speed", "SpeedMulti%i", "Speed Slot %i", 0, 9).ToList();
            multi.ForEach(x => x.displayCondition = speedEnabled);

            var initial = AddInt("Speed", "SpeedMultiInitial", "Initial speed at game startup", "");
            var playback = AddBool("Speed", "AffectPlayback", "Affect the playback speed of mve video files without an audio track", "");
            initial.style = RenderStyle.Inline;
            initial.displayCondition = speedEnabled;
            playback.displayCondition = speedEnabled;
        }

        public void InitSound()
        {
            AddInt("Sound", "NumSoundBuffers", "Number of sound buffers", "%C_ALL");
            var dsound = AddIntEnum("Sound", "AllowDShowSound", "DirectShow Sound", "%C_3", new Dictionary<string, int>
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

            AddBool("Sound", "AllowSoundForFloats", "Allow sound files for combat float messages", "");
        }

        public void InitGraphics()
        {
            AddIntEnum("Graphics", "Mode", "Graphics mode", "%C_4", new Dictionary<string, int>
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

            // We use this in the resolution control below
            AddInt("Graphics", "GraphicsWidth", "", "").dontRender = true;
            AddInt("Graphics", "GraphicsHeight", "", "").dontRender = true;

            var isDxEnabled = "Graphics::Mode > 0";

            var r = AddResolution("Graphics", "DX Resolution", "GraphicsWidth,GraphicsHeight",
                "If using a DX9 mode, this changes the resolution.\nThe graphics are simply stretched to fit the new window; this does _not_ let you see more of the map\nIf set to 0, use Fallout's native resolution.");
            r.displayCondition = isDxEnabled;

            var h = AddBool("Graphics", "Use32BitHeadGraphics", "Allow using 32-bit textures for talking heads", "%C_2-4");
            h.displayCondition = isDxEnabled;

            var dmovies = AddBool("Graphics", "AllowDShowMovies", "DirectShow movies", "Automatically search for alternative avi video files when Fallout tries to play the game movies.");
            dmovies.displayCondition = isDxEnabled;

            AddInt("Graphics", "FadeMultiplier", "Fade multiplier", "Fade effect time percentage modifier\nDefault is 100. Decrease/increase this value to speed up/slow down fade effects");
        }

        public void InitInterface()
        {
            var expandedDesc = "You can use resized FRMs in 700x682 for town maps in the expanded world map interface\n" +
                "Requires High Resolution Patch v4.1.8 and a new WORLDMAP.frm file in art\\intrface\\ (included in sfall.dat)\n" +
                "The resolution of hi-res patch must be set to at least 890x720";

            var expanded = AddIntEnum("Interface", "ExpandWorldMap", "Expanded WorldMap", expandedDesc, new Dictionary<string, int>
            {
                { "Disabled", 0 },
                { "Expanded world map interface", 1 },
                { "Expanded world map interface + skip correcting entrance markers", 2 },
            });
            expanded.valueDescription = @"1:Use the expanded world map interface:" +
                                    "2:Use the expanded world map interface + skip correcting the position of entrance markers on town maps:";

            var actionbarDesc = "Requires new IFACE_E.frm and HR_IFACE_<res>E.frm files in art\\intrface\\ (included in sfall.dat) to display correctly\n" +
                "The minimum supported version of High Resolution Patch is 4.1.8";

            AddBool("Interface", "ActionPointsBar", "Expand the number of action points displayed on the interface bar", actionbarDesc);
            AddBool("Interface", "WorldTravelMarkers", "Dots when travelling on worldmap", "Enable drawing a dotted line when moving around on the world map (similar to Fallout 1)");
        }

        public void InitInput()
        {
            AddBool("Input", "UseScrollWheel", "Enable the mouse scroll wheel to scroll through the inventory, barter, and loot screens", "");
            AddInt("Input", "ScrollMod", "Scroll modifier", "The mouse Z position is divided by this modifier to calculate the number of inventory slots to scroll.");
            AddInt("Input", "MouseSensitivity", "Mouse sensitivity", "%C_ALL");
            AddString("Input", "MiddleMouse", "Middle mouse scancode", "%C_1-2");
            AddBool("Input", "ReverseMouseButtons", "Reverse left and right mouse buttons", "");
            AddLabel("Input", "Enable these if you want Fallout to access the keyboard or mouse in background mode:");

            AddBool("Input", "BackgroundKeyboard", "Keyboard in background.", "");
            AddBool("Input", "BackgroundMouse", "Mouse in background.", "");
        }

        public void InitMisc()
        {
            AddBool("Misc", "Fallout1Behavior", "Enable Fallout 1 behavior.", "%C_ALL");
            var limit = AddInt("Misc", "TimeLimit", "Time limit in years", "%C_ALL");
            limit.min = -3;
            limit.max = 13;
            AddInt("Misc", "WorldMapTimeMod", "World map travel time percentage modifier", "%C_ALL");
            AddBool("Misc", "WorldMapFPSPatch", "Use the Fallout 1 code to control world map speed", null);
            var delay2 = AddInt("Misc", "WorldMapDelay2", "World map speed (Fallout 1 code)", "Controls the world map speed if Fallout 1 world map speed is enabled.\nHigher values cause slower movement");
            delay2.displayCondition = "Misc::WorldMapFPSPatch > 0";
            AddBool("Misc", "WorldMapEncounterFix", "Make world map encounter rate independent of your travel speed", null);
            var encRate = AddInt("Misc", "WorldMapEncounterRate", "Encounter rate", "Higher values of WorldMapEncounterRate cause a slower encounter rate");
            encRate.displayCondition = "Misc::WorldMapEncounterFix > 0";
            AddInt("Misc", "WorldMapSlots", "Worldmap slots", "%C_ALL");
            AddString("Misc", "StartingMap", "Starting map", "%C_ALL");
            AddString("Misc", "VersionString", "Version string", "%C_ALL");
        }

        public DDRAW()
        {
            AddBool("Main", "UseCommandLine", "Use Commandline", "Enable if you want to use command line args to tell sfall to use another ini file.");
            InitSpeed();
            InitSound();
            InitGraphics();
            InitInterface();
            InitInput();
            InitMisc();
        }
    }

    /*
     Some templating stuff:
        %C_ALL - Take all comment lines from ini (above that option).
        %C_3-5 - Take comment lines 3-5 from ini (above that option).
    */
    public class ini
    {
        public string Version;
        public string Path;

        public List<iniOption> options = new List<iniOption>();
        public List<IRender> renderables = new List<IRender>();
        //public Dictionary<string, List<string>> comments = new Dictionary<string, List<string>>();

        public Action redrawCallback;

        public iniOption FindIniOption(string unifiedName)
        {
            var spl = unifiedName.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
            return FindIniOption(spl[0], spl[1]);
        }

        public iniOption FindIniOption(string category, string name)
            => options.Where(z => z.category == category && z.name == name).SingleOrDefault();

        public void PreprocessDescription(iniOption opt)
        {
            if (opt.description == null)
                return;

            while (opt.description.Contains("%C_"))
            {
                var range = "";
                var idx = opt.description.IndexOf("%C_") + 2;
                while (true)
                {
                    if (opt.description[idx++] == ' ' || opt.description.Length == idx)
                        break;
                    range += opt.description[idx];
                }
                if(!range.Contains("ALL"))
                {
                    var modeSpl = range.Split('-');
                    var first = int.Parse(modeSpl[0]);
                    var second = modeSpl.Length == 1 ? first : int.Parse(modeSpl[1]);
                    var commentsToAdd = new List<string>();
                    for (var i = first-1; i <= second-1; i++)
                        commentsToAdd.Add(opt.comments[i]);

                    opt.description = opt.description.Replace("%C_"+range, string.Join("\n", commentsToAdd.ToArray()));
                }
                else
                {
                    opt.description = opt.description.Replace("%C_ALL", string.Join("\n", opt.comments));
                }
            }
        }

        public int GetInt(string category, string name)
        {
            var o = FindIniOption(category, name);
            if (o == null) return -1;
            return int.Parse(o.inidata);
        }

        iniOption BaseOption(string category, string name, optionType type, string label, string description)
        {
            return new iniOption()
            {
                ini = this,
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

        public iniLabel AddLabel(string category, string text)
        {
            var label = new iniLabel()
            {
                category = category,
                text = text
            };
            renderables.Add(label);
            return label;
        }

        public resolutionOption AddResolution(string category, string name, string iniOptions, string description)
        {
            var opt = new resolutionOption()
            {
                ini = this,
                category = category,
                name = name,
                description = description,
                iniOptions = iniOptions
            };
            renderables.Add(opt);
            return opt;
        }

        public void AddBoolString(string category, string name, string label, string description)
        {
            var opt = BaseOption(category, name, optionType.booleanString, label, description);
            AddOption(opt);
        }

        public void AddOption(iniOption opt)
        {
            options.Add(opt);
            renderables.Add(opt);
        }

        public iniOption AddBool(string category, string name, string label, string description)
        {
            var opt = BaseOption(category, name, optionType.boolean, label, description);
            AddOption(opt);
            return opt;
        }

        public IEnumerable<iniOption> AddIntRange(string category, string name, string label, int rBegin, int rEnd)
        {
            for(var i=rBegin;i<=rEnd;i++)
            {
                var opt = BaseOption(category, name.Replace("%i", i.ToString()), optionType.integer, label.Replace("%i", i.ToString()), "");
                opt.style = RenderStyle.Inline;
                AddOption(opt);
                yield return opt;
            }
        }

        public iniOption AddString(string category, string name, string label, string description)
        {
            var opt = BaseOption(category, name, optionType.str, label, description);
            AddOption(opt);
            return opt;
        }

        public iniOption AddInt(string category, string name, string label, string description)
        {
            var opt = BaseOption(category, name, optionType.integer, label, description);
            AddOption(opt);
            return opt;
        }

        public iniOption AddIntEnum(string category, string name, string label, string description, Dictionary<string, int> enums)
        {
            var opt = BaseOption(category, name, optionType.intEnum, label, description);
            opt.enums = enums;
            AddOption(opt);
            return opt;
        }

        public void Parse(string path)
        {
            Path = path;
            string category="";
            bool enabled = false;

            List<string> commentBuffer = new List<string>();

            foreach(var line in File.ReadAllLines(path))
            {
                if (line.Length == 0)
                {
                    commentBuffer = new List<string>();
                    continue;
                }

                if (line[0] == ';' && line.Length > 1 && line[1] == 'v')
                    Version = line.Substring(2);
                if (line[0] == '[')
                {
                    commentBuffer = new List<string>();
                    category = line.Substring(1, line.Length - 2);
                }

                //if (!line.Contains("="))
                //    continue;

                enabled = true;
                string keyValue = line;

                // commented option
                if(line[0]==';')
                {
                    enabled = false;
                    keyValue = line.Substring(1);

                   // var splComment = line.Split('=');
                    var isComment = line.IndexOf(' ') != -1;
                    if(isComment)
                        commentBuffer.Add(keyValue);
                }
                if (!line.Contains("="))
                    continue;

                
                var spl = keyValue.Split('=');
                var key = spl[0];
                var val = spl[1];

                var found = options.SingleOrDefault(x => x.category == category && x.name == key);
                if(found != null)
                {
                    found.inidata = val;
                    found.enabled = enabled;
                    found.found = true;
                    found.comments = commentBuffer;
                    PreprocessDescription(found);
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
        str,
        path,
    }

    public interface IRender
    {
        int Render(Control parent, int y);
        string GetCategory();
    };

    public class iniLabel : IRender
    {
        public string category;
        public string text;
        public string displayCondition;

        public string GetCategory() => category;
        public int Render(Control parent, int y)
        {
            var lblw = new Label();
            lblw.Text = text;
            lblw.Parent = parent;
            lblw.Location = new Point(10, y);
            lblw.AutoSize = true;
            y += 20;
            return y;
        }
    }

    public class resolutionOption : IRender
    {
        public ini ini;
        public string name;
        public string description;
        public string category;
        public string iniOptions;
        public string displayCondition;

        public string GetCategory() => category;

        public int Render(Control parent, int y)
        {
            if (!string.IsNullOrEmpty(displayCondition))
            {
                if (!DisplayConditionParse.IsConditionTrue(displayCondition, ini))
                    return y;
            }

            var spl = iniOptions.Split(',');
            var wOpt = ini.FindIniOption(category, spl[0]);
            var hOpt = ini.FindIniOption(category, spl[1]);

            var width = int.Parse(wOpt.inidata);
            var height = int.Parse(hOpt.inidata);

            var grp = new GroupBox();
            grp.Text = name;
            grp.Parent = parent;
            grp.Location = new Point(10, y);
            grp.AutoSize = true;

            int grpY = 25;
            int x = 10;

            var lblw = new Label();
            lblw.Text = "W:";
            lblw.Parent = grp;
            lblw.Location = new Point(x, grpY+3);
            lblw.AutoSize = true;

            x += 23;

            var num1 = new NumericUpDown();
            num1.Width = 50;
            num1.Location = new Point(x, grpY);
            num1.Maximum = 10000;
            num1.Value = width;
            num1.Parent = grp;
            num1.ValueChanged += (object sender, EventArgs e) => wOpt.inidata = num1.Value.ToString();
            
            x += 55;

            var lblh = new Label();
            lblh.Text = "H:";
            lblh.Parent = grp;
            lblh.Location = new Point(x, grpY+3);
            lblh.AutoSize = true;

            x += 23;

            var num2 = new NumericUpDown();
            num2.Width = 50;
            num2.Location = new Point(x, grpY);
            num2.Maximum = 10000;
            num2.Value = height;
            num2.Parent = grp;
            num2.ValueChanged += (object sender, EventArgs e) => wOpt.inidata = num1.Value.ToString();

            if (!string.IsNullOrEmpty(description))
            {
                x = 10;
                grpY += 25;
                var dsc = new Label();
                dsc.AutoSize = true;
                dsc.Text = description;
                dsc.Location = new Point(x, grpY);
                dsc.Parent = grp;
            }

            grp.Height = grpY;

            y += grp.Height + 20;

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

        public static bool IsConditionTrue(string displayCondition, ini ini)
        {
            var parsed = new DisplayConditionParse(displayCondition);
            var opt = ini.FindIniOption(parsed.category, parsed.name);
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

    public enum RenderStyle
    {
        Auto,
        Inline
    }

    public class iniOption : IRender
    {
        public ini ini;
        public string category;
        public string label;
        public string name;
        public string description;
        public optionType type;
        public RenderStyle style;
        public bool found;
        public bool enabled;
        public bool dontRender; // if used in a multifield option.
        public int? min; // only for int
        public int? max; // only for int
        public string inidata;
        public string displayCondition;
        public string valueDescription;
        public Dictionary<string, int> enums;
        public List<string> comments;

        public string GetCategory() => category;

        private Label CreateLabel(Control parent, string text, int x, int y)
        {
            var label = new Label();
            label.AutoSize = true;
            label.Text = text;
            label.Location = new Point(x, y);
            label.Parent = parent;
            return label;
        }

        private GroupBox CreateGroupBox(Control parent, int x, int y)
        {
            var grp = new GroupBox();
            grp.Text = label;
            grp.Parent = parent;
            grp.Location = new Point(x, y);
            grp.AutoSize = true;
            return grp;
        }

        private NumericUpDown CreateNumericUpDown(Control parent, int x, int y)
        {
            var num = new NumericUpDown();
            num.Width = 50;
            num.Location = new Point(x, y);
            num.Minimum = -100000;
            num.Maximum = 100000;
            num.Value = int.Parse(inidata);
            num.Parent = parent;
            num.ValueChanged += (object sender, EventArgs e) => inidata = num.Value.ToString();
            return num;
        }

        public int Render(Control parent, int y)
        {
            if (dontRender)
                return y;

            if(!string.IsNullOrEmpty(displayCondition))
            {
                if (!DisplayConditionParse.IsConditionTrue(displayCondition, ini))
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
                    ini.redrawCallback();
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

                y += 20;
            }
            if (type == optionType.integer)
            {
                // Draw everything on one line
                if (style == RenderStyle.Inline)
                {
                    var lbl = CreateLabel(parent, label, x, y+3);
                    CreateNumericUpDown(parent, lbl.PreferredWidth+x, y);
                    y += 30;
                }
                else
                {
                    var grp = CreateGroupBox(parent, x, y);
                    int grpY = 25;
                    var num = CreateNumericUpDown(grp, x, grpY);
                    x = 10;

                    if (min.HasValue)
                        num.Minimum = min.Value;
                    if (max.HasValue)
                        num.Maximum = max.Value;

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
                    y += grp.Height + 10;
                }
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
                    // TODO: Be more clever about when to redraw or not
                    // we can check if some displayCondition is depending on this.
                    ini.redrawCallback();
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

                y += grp.Height + 10;
            }

            if (type == optionType.str)
            {
                var grp = this.CreateGroupBox(parent, x, y);
                int grpY = 20;

                /* var lbl = CreateLabel(parent, label, x, grpY + 3);
                x += lbl.Width;*/
                var txt = new TextBox();
                txt.Location = new Point(x, grpY);
                txt.Parent = grp;
                txt.Text = inidata;
                txt.TextChanged += (object sender, EventArgs e) =>
                {
                    this.inidata = txt.Text;
                };
                if (!string.IsNullOrEmpty(description))
                {
                    x = 10;
                    grpY += 25;
                    var label = new Label();
                    label.AutoSize = true;
                    label.Text = description;
                    label.Location = new Point(x, grpY);
                    label.Parent = grp;
                }

                y += grp.Height + 10;
            }




            return y;
        }
    }
}
