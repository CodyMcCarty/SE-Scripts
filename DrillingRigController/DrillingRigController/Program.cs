using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        readonly float cargoVolumeLimitPercent = 50f;
        readonly float downSpeed = .04f;
        readonly string groupName = "Drilling Rig";
        readonly string cargoName = "Cargo_DR";
        readonly string drillName = "DR_Drill";
        readonly string pistonUpName = "PistonMast";
        readonly string pistonAcrossName = "PistonJib";
        readonly string pistonOnDrillName = "PistonDriver";
        List<IMyTerminalBlock> drillingRig = new List<IMyTerminalBlock>();
        List<IMyShipDrill> drills = new List<IMyShipDrill>();
        List<IMyPistonBase> mast = new List<IMyPistonBase>();
        List<IMyPistonBase> jib = new List<IMyPistonBase>();
        List<IMyPistonBase> kelly = new List<IMyPistonBase>();
        IMyCargoContainer cargo;

        public Program()
        {
            Echo("Loading Drill Rig...");

            //assign Cargo container
            cargo = (IMyCargoContainer)GridTerminalSystem.GetBlockWithName(cargoName);
            if (cargo == null) PrintNotFound(cargoName);

            //setup
            IMyBlockGroup rigGroup = GridTerminalSystem.GetBlockGroupWithName(groupName);
            if (rigGroup == null) PrintNotFound(groupName);
            drillingRig.Clear();
            rigGroup.GetBlocks(drillingRig);
            drills.Clear();
            mast.Clear();
            jib.Clear();
            kelly.Clear();
            //assignments
            foreach (var block in drillingRig)
            {
                if (block.CustomName.Contains(drillName)) drills.Add(block as IMyShipDrill);
                if (block.CustomName.Contains(pistonUpName)) mast.Add(block as IMyPistonBase);
                if (block.CustomName.Contains(pistonAcrossName)) jib.Add(block as IMyPistonBase);
                if (block.CustomName.Contains(pistonOnDrillName)) kelly.Add(block as IMyPistonBase);
            }
            downSpeed /= (mast.Count() + kelly.Count());
            PreDrillChecks();
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            Echo("Drilling rig ready");
        }

        public void Main(string argument, UpdateType updateSource)
        {
            PreDrillChecks();
            Echo($"Cargo capacity at '{CargoVolume()}'%. \nDrilling will Stop at '{cargoVolumeLimitPercent}'% or more.");

            if (CargoVolume() >= cargoVolumeLimitPercent) StopDrilling();
            else StartDrilling();
            //TODO: if the drills reach the bottom, then bring them back up
        } //END Main()

        void StartDrilling()
        {
            Echo("Drilling...");
            TurnDrillsOn();
            SetAllPistonsVelocity("down");
        }

        void StopDrilling()
        {
            //TODO: bring pistons up some before stopping to avoid breaking
            //drill due to time delay on drill firing up
            SetAllPistonsVelocity("up");
            Echo("Drilling Stoped");
            SetAllPistonsVelocity("stop");
            TurnDrillsOff();
        }

        float CargoVolume()
        {
            float howFull = 100.0f * (float)cargo.GetInventory(0).CurrentVolume / (float)cargo.GetInventory(0).MaxVolume;
            return howFull;
        }

        void SetAllPistonsVelocity(string direction)
        {
            float velocity = 0.0f;
            if (direction == "down") velocity = downSpeed;
            //TODO: up velocity to 5 once confirm working
            if (direction == "up") velocity = -5.0f;

            foreach (IMyPistonBase p in mast) p.Velocity = velocity * -1;
            foreach (IMyPistonBase p in kelly) p.Velocity = velocity;
        }

        void TurnDrillsOn()
        {
            foreach (IMyShipDrill drill in drills) drill.ApplyAction("OnOff_On");
        }

        void TurnDrillsOff()
        {
            foreach (IMyShipDrill drill in drills) drill.ApplyAction("OnOff_Off");
        }

        void PreDrillChecks()
        {
            //TODO: add Checks
            if (mast.Count() == 0) PrintNotFound(pistonUpName);
            if (drills.Count() == 0) PrintNotFound(drillName);
            if (kelly.Count() == 0) PrintNotFound(pistonOnDrillName);
        }

        void PrintNotFound(string name)
        {
            //TODO: add type: info, warn, err
            Echo($"ERROR: Missing '{name}` or is empty");
            return;
        }


    }
}
