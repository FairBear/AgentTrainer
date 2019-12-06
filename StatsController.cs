using AIProject;
using CharaCustom;
using ExtensibleSaveFormat;
using KKAPI;
using KKAPI.Chara;
using MessagePack;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AgentTrainer
{
	public class StatsController : CharaCustomFunctionController
	{
		public int id;
		public AgentActor agent;

		public Dictionary<int, float> lockedStats = new Dictionary<int, float>();
		public Dictionary<int, float> lockedDesires = new Dictionary<int, float>();
		public Dictionary<int, int> lockedFlavors = new Dictionary<int, int>();

		protected override void OnCardBeingSaved(GameMode currentGameMode)
		{
			if (lockedStats.Count == 0 &&
				lockedDesires.Count == 0 &&
				lockedFlavors.Count == 0)
			{
				SetExtendedData(null);
				return;
			}

			PluginData data = new PluginData
			{
				version = 1
			};

			data.data.Add("lockedStats", LZ4MessagePackSerializer.Serialize(lockedStats));
			data.data.Add("lockedDesires", LZ4MessagePackSerializer.Serialize(lockedDesires));
			data.data.Add("lockedFlavors", LZ4MessagePackSerializer.Serialize(lockedFlavors));

			SetExtendedData(data);
		}

		protected override void OnReload(GameMode currentGameMode, bool maintainState)
		{
			if (CustomBase.IsInstance() &&
				CustomBase.Instance.chaCtrl == ChaControl)
			{
				CustomCharaWindow[] customCharaWindows =
					FindObjectsOfType<CustomCharaWindow>();

				if (customCharaWindows != null && customCharaWindows.Length >= 2)
				{
					var toggles = new HarmonyLib.Traverse(customCharaWindows[1])
						.Field("tglLoadOption")
						.GetValue<Toggle[]>();

					if (toggles != null &&
						toggles.Length >= 5 &&
						!toggles[4].isOn)
						return;
				}
			}

			PluginData data = GetExtendedData();

			if (data != null)
			{
				Dictionary<int, float> newLockedStats;
				Dictionary<int, float> newLockedDesires;
				Dictionary<int, int> newLockedFlavors;

				try
				{
					newLockedStats = LZ4MessagePackSerializer
						.Deserialize< Dictionary<int, float>>((byte[])data.data["lockedStats"]);
					newLockedDesires = LZ4MessagePackSerializer
						.Deserialize<Dictionary<int, float>>((byte[])data.data["lockedDesires"]);
					newLockedFlavors = LZ4MessagePackSerializer
						.Deserialize<Dictionary<int, int>>((byte[])data.data["lockedFlavors"]);
				}
				catch (Exception err)
				{
					Debug.Log($"[Agent Trainer] Failed to load extended data.\n{err}");
					return;
				}

				lockedStats = newLockedStats;
				lockedDesires = newLockedDesires;
				lockedFlavors = newLockedFlavors;
			}
		}

		protected override void Start()
		{
			base.Start();
			AgentTrainer.AddController(this);
		}

		void LateUpdate()
		{
			if (agent == null)
				return;

			Dictionary<int, float> stats = agent.AgentData.StatsTable;

			foreach (KeyValuePair<int, float> stat in lockedStats)
				stats[stat.Key] = stat.Value;

			Dictionary<int, float> desires = agent.AgentData.DesireTable;

			foreach (KeyValuePair<int, float> desire in lockedDesires)
				desires[desire.Key] = desire.Value;

			Dictionary<int, int> flavors = ChaControl.fileGameInfo.flavorState;

			foreach (KeyValuePair<int, int> flavor in lockedFlavors)
				if (flavors[flavor.Key] != flavor.Value)
					agent.SetFlavorSkill(flavor.Key, flavor.Value);
		}
	}
}
