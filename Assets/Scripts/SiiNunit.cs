using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace fs {
    public class SiiNunit {
        public Dictionary<string,Unit> unit;
        public Dictionary<string,Unit> root;

        public SiiNunit(string path) {
            unit=new Dictionary<string,Unit>();
            StreamReader sr = new StreamReader(path);
            string temp = sr.ReadLine();
            if(temp.Contains("SiiNunit")) {
                if(!temp.Contains("{")) {
                    temp=sr.ReadLine();
                    if(!temp.Contains("{")) {
                        throw new Exception("不是有效的SiiNunit");
                    }
                }
            } else {
                throw new Exception("不是有效的SiiNunit");
            }

            bool exit = false;
            while(!sr.EndOfStream) {
                int t;
                StringBuilder sbName = new StringBuilder();

                while((t=sr.Read())!='{') {
                    if(sr.EndOfStream) { exit=true; break; }
                    sbName.Append((char)t);
                }
                if(exit) break;

                string cName = sbName.ToString().Replace("\n","").Replace("\r","");

                string detail = null;
                if(t == '{') {
                    //开始unit
                    StringBuilder sb = new StringBuilder();
                    while((t=sr.Read())!='}') {
                        if(sr.EndOfStream) { exit=true; break; }
                        sb.Append((char)t);
                    }
                    if(exit) break;
                    detail=sb.ToString();
                }

                int ind = cName.LastIndexOf(':');
                string className = cName.Substring(0,ind).Trim();
                Dictionary<string,string> dic = GetKeyValues(detail);
                System.Diagnostics.Debug.WriteLine(className);
                Unit a;
                switch(className) {
                    case "ui::window": a = new UI_Window(); break;
                    case "ui::group": a = new UI_Group(); break;
                    case "ui::text_common": a = new UI_TextCommon(); break;
                    case "ui::text": a = new UI_Text(); break;
                    case "ui::frame_common": a = new UI_FrameCommon(); break;
                    case "ui::button": a = new UI_Button(); break;
                    case "ui::button_common": a = new UI_ButtonCommon(); break;
                    case "ui_blinking_button": a = new Ui_BlinkingButton(); break;
                    case "main_menu": a = new MainMenu(); break;
                    case "loading": a = new Loading(); break;
                    case "intro_menu": a = new IntroMenu(); break;
                    case "bug_report_hdl": a = new BugReport(); break;
                    case "options_menu": a = new OptionsMenu(); break;
                    case "shared_menu": a = new SharedMenu(); break;
                    case "mission_selection": a = new MissionSelection(); break;
                    default: a= new CommonUnit(); break;
                };
                a.className=className;
                a.unitName=cName.Substring(ind+1,cName.Length-ind-1).Trim();
                a.item=GetKeyValues(detail);
                unit.Add(a.unitName,a);
            }

            sr.Close();
            LinkNodes();
        }
        private void LinkNodes() {
            foreach(KeyValuePair<string,Unit> kvp in unit) {
                kvp.Value.Init(unit);
            }

            root=new Dictionary<string,Unit>();
            foreach(KeyValuePair<string,Unit> kvp in unit) {
                if(!kvp.Value.item.ContainsKey("my_parent")) {
                    root.Add(kvp.Key,kvp.Value);
                } else {
                    kvp.Value.item.TryGetValue("my_parent",out string na);
                    if(na.Equals("null")) root.Add(kvp.Key,kvp.Value);
                }
            }
        }

        private Dictionary<string,string> GetKeyValues(string total) {
            if(string.IsNullOrEmpty(total)) return null;
            string[] ts = total.Split('\n');
            string temp;
            Dictionary<string,string> keys = new Dictionary<string,string>();
            for(int i = 0;i<ts.Length;i++) {
                temp=ts[i];
                if(temp.Contains("#")) {
                    int index = temp.IndexOf('#');
                    if(index==0) continue;
                    temp=temp.Substring(0,index);
                    if(temp.Length<1) continue; //去注释
                }

                if(temp.Contains(":")) {
                    //属性行
                    int index = temp.IndexOf(':');
                    string keyName = temp.Substring(0,index).Trim();
                    string value = temp.Substring(index+1,temp.Length-index-1).Trim();
                    keys.Add(keyName,value);
                }
            }
            return keys;
        }

        public enum UnitType { ui_window, ui_group };


        public abstract class Unit {
            public string className;
            public string unitName;
            public Dictionary<string,string> item;

            public override string ToString() {
                return className+":"+unitName;
            }
            public abstract void Init(Dictionary<string,Unit> unit);
        }
        public class CommonUnit:Unit {
            public override void Init(Dictionary<string,Unit> unit) {
                System.Diagnostics.Debug.WriteLine("未知类");
            }
        }

        public abstract class UI:Unit {
            public int coords_b;
            public int coords_l;
            public int coords_r;
            public int coords_t;
            public override void Init(Dictionary<string,Unit> unit) {
                string temp;
                if(item.TryGetValue("coords_b",out temp)) coords_b=GetNum(temp);
                if(item.TryGetValue("coords_l",out temp)) coords_l=GetNum(temp);
                if(item.TryGetValue("coords_r",out temp)) coords_r=GetNum(temp);
                if(item.TryGetValue("coords_t",out temp)) coords_t=GetNum(temp);

            }
            private int GetNum(string temp) {
                if(temp.IndexOf('&')<0) return int.Parse(temp);
                else {
                    temp = temp.Replace("&","");
                    byte[] bytes = new byte[4];
                    bytes[0]=Convert.ToByte(temp.Substring(0,2),16);
                    bytes[1]=Convert.ToByte(temp.Substring(2,2),16);
                    bytes[2]=Convert.ToByte(temp.Substring(4,2),16);
                    bytes[3]=Convert.ToByte(temp.Substring(6,2),16);

                    float f = BitConverter.ToSingle(bytes,0);
                    return (int)f;
                }
            }

            protected Unit[] GetMyChildren(Dictionary<string,Unit> unit) {
                List<Unit> myChildren = new List<Unit>();
                int i = 0;
                while(item.TryGetValue("my_children["+i+"]",out string temp)) {
                    if(unit.TryGetValue(temp,out Unit aa)) {
                        myChildren.Add(aa);
                    } else {
                        System.Diagnostics.Debug.WriteLine("缺失unit");
                    }
                    i++;
                }
                return myChildren.ToArray();
            }
        }


        public class UI_Window:UI {
            public new static readonly string className = "ui::window";
            public Unit[] children;
            public string name;
            public Unit window_handler;

            public override void Init(Dictionary<string,Unit> unit) {
                base.Init(unit);
                string temp;
                children=GetMyChildren(unit);
                if(item.TryGetValue("name",out temp)) name=temp;
                if(item.TryGetValue("window_handler",out temp)) {
                    unit.TryGetValue(temp,out window_handler);
                }
            }
        }

        public class UI_Group:UI {
            public new static readonly string className = "ui::group";
            public bool fitting;
            public int id;
            public int layer;
            public Unit[] children;
            public Unit parent;
            public string name;

            public override void Init(Dictionary<string,Unit> unit) {
                base.Init(unit);
                string temp;
                if(item.TryGetValue("fitting",out temp)) fitting=bool.Parse(temp);
                if(item.TryGetValue("id",out temp)) id=int.Parse(temp);
                if(item.TryGetValue("layer",out temp)) layer=int.Parse(temp);
                children=GetMyChildren(unit);
                if(item.TryGetValue("my_parent",out temp)) {
                    unit.TryGetValue(temp,out parent);
                }
                if(item.TryGetValue("name",out temp)) name=temp;
            }
        }

        public class UI_TextCommon:UI {
            public new static readonly string className = "ui::text_common";
            public int id;
            public int layer;
            public string look_template;
            public Unit parent;
            public string value;

            public override void Init(Dictionary<string,Unit> unit) {
                base.Init(unit);
                string temp;
                if(item.TryGetValue("id",out temp)) id=int.Parse(temp);
                if(item.TryGetValue("layer",out temp)) layer=int.Parse(temp);
                if(item.TryGetValue("look_template",out temp)) look_template=temp;

                if(item.TryGetValue("my_parent",out temp)) {
                    unit.TryGetValue(temp,out parent);
                }
                if(item.TryGetValue("value",out temp)) value=temp;
            }
        }

        public class UI_Text:UI {
            public new static readonly string className = "ui::text";
            public int layer;
            public Unit parent;
            public string name;
            public string text;

            public override void Init(Dictionary<string,Unit> unit) {
                base.Init(unit);
                string temp;
                if(item.TryGetValue("layer",out temp)) layer=int.Parse(temp);

                if(item.TryGetValue("my_parent",out temp)) {
                    unit.TryGetValue(temp,out parent);
                }
                if(item.TryGetValue("name",out temp)) name=temp;
                if(item.TryGetValue("text",out temp)) text=temp;
            }
        }

        public class UI_FrameCommon:UI {
            public new static readonly string className = "ui::frame_common";
            public int layer;
            public string lookTemplate;
            public Unit parent;

            public override void Init(Dictionary<string,Unit> unit) {
                base.Init(unit);
                string temp;
                if(item.TryGetValue("layer",out temp)) layer=int.Parse(temp);
                if(item.TryGetValue("look_template",out temp)) lookTemplate=temp;

                if(item.TryGetValue("my_parent",out temp)) {
                    unit.TryGetValue(temp,out parent);
                }
            }
        }

        public class UI_Button:UI {
            public new static readonly string className = "ui::button";
            public int id;
            public int layer;
            public Unit parent;
            public string name;

            public override void Init(Dictionary<string,Unit> unit) {
                base.Init(unit);
                string temp;
                if(item.TryGetValue("id",out temp)) id=int.Parse(temp);
                if(item.TryGetValue("layer",out temp)) layer=int.Parse(temp);

                if(item.TryGetValue("my_parent",out temp)) {
                    unit.TryGetValue(temp,out parent);
                }
                if(item.TryGetValue("name",out temp)) name=temp;
            }
        }
        public class UI_ButtonCommon:UI {
            public new static readonly string className = "ui::button_common";
            public int id;
            public int layer;
            public Unit parent;
            public string name;

            public override void Init(Dictionary<string,Unit> unit) {
                base.Init(unit);
                string temp;
                if(item.TryGetValue("id",out temp)) id=int.Parse(temp);
                if(item.TryGetValue("layer",out temp)) layer=int.Parse(temp);

                if(item.TryGetValue("my_parent",out temp)) {
                    unit.TryGetValue(temp,out parent);
                }
                if(item.TryGetValue("name",out temp)) name=temp;
            }
        }

        public class Ui_BlinkingButton:UI_Button {
            public new static readonly string className = "ui_blinking_button";
            public string value;
            public string lookTemplate;

            public override void Init(Dictionary<string,Unit> unit) {
                base.Init(unit);
                string temp;
                if(item.TryGetValue("value",out temp)) value=temp;
                if(item.TryGetValue("look_template",out temp)) lookTemplate=temp;
            }
        }

        public class MainMenu:Unit {
            public new static readonly string className = "main_menu";
            public Unit windowLink;

            public override void Init(Dictionary<string,Unit> unit) {
                if(item.TryGetValue("window_link",out string temp)) {
                    unit.TryGetValue(temp,out windowLink);
                }
            }
        }
        public class Loading:Unit {
            public new static readonly string className = "loading";
            public Unit windowLink;

            public override void Init(Dictionary<string,Unit> unit) {
                if(item.TryGetValue("window_link",out string temp)) {
                    unit.TryGetValue(temp,out windowLink);
                }
            }
        }
        public class IntroMenu:Unit {
            public new static readonly string className = "intro_menu";
            public Unit windowLink;

            public override void Init(Dictionary<string,Unit> unit) {
                if(item.TryGetValue("window_link",out string temp)) {
                    unit.TryGetValue(temp,out windowLink);
                }
            }
        }
        public class BugReport:Unit {
            public new static readonly string className = "bug_report_hdl";
            public Unit windowLink;

            public override void Init(Dictionary<string,Unit> unit) {
                if(item.TryGetValue("window_link",out string temp)) {
                    unit.TryGetValue(temp,out windowLink);
                }
            }
        }
        public class OptionsMenu:Unit {
            public new static readonly string className = "options_menu";
            public Unit windowLink;

            public override void Init(Dictionary<string,Unit> unit) {
                if(item.TryGetValue("window_link",out string temp)) {
                    unit.TryGetValue(temp,out windowLink);
                }
            }
        }
        public class SharedMenu:Unit {
            public new static readonly string className = "shared_menu";
            public Unit windowLink;

            public override void Init(Dictionary<string,Unit> unit) {
                if(item.TryGetValue("window_link",out string temp)) {
                    unit.TryGetValue(temp,out windowLink);
                }
            }
        }
        public class MissionSelection:Unit {
            public new static readonly string className = "mission_selection";
            public Unit windowLink;

            public override void Init(Dictionary<string,Unit> unit) {
                if(item.TryGetValue("window_link",out string temp)) {
                    unit.TryGetValue(temp,out windowLink);
                }
            }
        }
    }

}
