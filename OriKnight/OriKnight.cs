using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;
using System.IO;
using System.Reflection;
 
using UObject = UnityEngine.Object;


namespace OriKnight
{
    public class OriKnight : Mod
    {
        internal static OriKnight Instance;

        public override void Initialize()//Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing OriKnight");
            Instance = this;
            //Skills.ChargedJump.Hook();
            //Skills.Grenade.Hook();
            //Skills.Glide.Hook();
            //Skills.Launch.Hook();
            //Skills.Bash.Hook();
            Skills.Shuriken.Hook();

            //DirectoryInfo dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            //string binds = File.OpenText(Path.Combine(dir.FullName)+"\\binds.txt").ReadToEnd();
            
            //Skills.Bash.bashButton = (KeyCode)System.Enum.Parse(typeof(KeyCode), binds.Split(':')[1].Trim());
            //Skills.Glide.glideButton = (KeyCode)System.Enum.Parse(typeof(KeyCode), binds.Split(':')[3].Trim());


        }




        public override string GetVersion() => "0.0.0.3";
    }
}