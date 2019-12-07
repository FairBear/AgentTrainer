using AIChara;
using AIProject;
using AIProject.Definitions;
using CharaCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Sickness = AIProject.SaveData.Sickness;

namespace AgentTrainer
{
	public partial class AgentTrainer
	{
		const string HEADER = "Agent Trainer";

		const float MARGIN_TOP = 20f;
		const float MARGIN_BOTTOM = 10f;
		const float MARGIN_LEFT = 10f;
		const float MARGIN_RIGHT = 10f;
		const float WIDTH = 540f;
		const float HEIGHT = 460f;
		const float INNER_WIDTH = WIDTH - MARGIN_LEFT - MARGIN_RIGHT;
		const float INNER_HEIGHT = HEIGHT - MARGIN_TOP - MARGIN_BOTTOM;

		const float ENTRY_WIDTH = 200f;
		const float AGENTS_WIDTH = 120f;
		const float LABEL_WIDTH = 90f;
		const float VALUE_WIDTH = 40f;

		static Rect rect = new Rect(
			Screen.width - WIDTH,
			(Screen.height - HEIGHT) / 2,
			WIDTH,
			HEIGHT
		);
		static Rect innerRect = new Rect(
			MARGIN_LEFT,
			MARGIN_TOP,
			INNER_WIDTH,
			INNER_HEIGHT
		);
		static Rect dragRect = new Rect(0f, 0f, WIDTH, 20f);

		GUIStyle sectionLabelStyle;
		GUIStyle selectedButtonStyle;

		readonly Dictionary<Desire.Type, int> desireTable = typeof(Desire).GetField(
			"_desireKeyTable",
			BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
		).GetValue(null) as Dictionary<Desire.Type, int>;

		readonly List<string> tabs = new List<string>()
		{
			"Info",
			"Sliders"
		};
		int tab = 0;

		bool visible = false;

		void OnGUI()
		{
			if (!visible)
				return;

			if (sectionLabelStyle == null)
			{
				sectionLabelStyle = new GUIStyle(GUI.skin.label)
				{
					fontStyle = FontStyle.Bold
				};

				selectedButtonStyle = new GUIStyle(GUI.skin.button)
				{
					fontStyle = FontStyle.Bold,
					normal = {
						textColor = Color.red
					},
					focused = {
						textColor = Color.red
					},
					active = {
						textColor = Color.red
					},
					hover = {
						textColor = Color.red
					},
				};
			}

			rect = GUI.Window(
				WindowID.Value,
				rect,
				Draw,
				HEADER
			);
		}

		float Draw_Contents_Slider(string label,
								   bool locked,
								   float value,
								   float min,
								   float max,
								   out bool toggle)
		{
			GUILayout.BeginHorizontal();
			{
				toggle = GUILayout.Button(
					label,
					locked ? selectedButtonStyle : GUI.skin.button,
					GUILayout.Width(LABEL_WIDTH)
				);
				value = GUILayout.HorizontalSlider(value, min, max);

				GUILayout.Label($"{value:F0}", GUILayout.Width(VALUE_WIDTH));
			}
			GUILayout.EndHorizontal();

			return value;
		}

		float Draw_Contents_Slider(Dictionary<int, float> dict,
								   int key,
								   string label,
								   float value,
								   float min,
								   float max)
		{
			bool locked = dict.ContainsKey(key);

			if (locked)
				value = dict[key];

			float next = Draw_Contents_Slider(
				label,
				locked,
				value,
				min,
				max,
				out bool toggle
			);

			if (toggle)
				if (locked)
					dict.Remove(key);
				else
					dict[key] = next;
			else if (locked)
				dict[key] = next;

			return next;
		}

		int Draw_Contents_Slider(Dictionary<int, int> dict,
								 int key,
								 string label,
								 int value,
								 float min,
								 float max)
		{
			int next = (int)Draw_Contents_Slider(
				label,
				dict.ContainsKey(key),
				value,
				min,
				max,
				out bool toggle
			);

			if (toggle)
				if (dict.ContainsKey(key))
					dict.Remove(key);
				else
					dict[key] = next;
			else if (dict.ContainsKey(key))
				dict[key] = next;

			return next;
		}

		void Draw_Sliders_Stats()
		{
			if (controller.agent == null)
				return;

			GUILayout.Label("Stats", sectionLabelStyle);

			Dictionary<int, float> stats = controller.agent.AgentData.StatsTable;

			foreach (int stat in stats.Keys.ToList())
			{
				float curr = stats[stat];
				float next = Draw_Contents_Slider(
					controller.lockedStats,
					stat,
					((Status.Type)stat).ToString(),
					curr,
					0f,
					100f
				);

				if (next != curr)
					stats[stat] = next;
			}
		}

		void Draw_Sliders_Flavors()
		{
			GUILayout.Label("Flavors", sectionLabelStyle);

			Dictionary<int, int> flavors = controller.ChaControl.fileGameInfo.flavorState;

			foreach (int flavor in flavors.Keys.ToList())
			{
				int curr = flavors[flavor];
				int next = Draw_Contents_Slider(
					controller.lockedFlavors,
					flavor,
					((FlavorSkill.Type)flavor).ToString(),
					curr,
					0f,
					9999f
				);

				if (next != curr)
					if (controller.agent != null)
						controller.agent.SetFlavorSkill(flavor, next);
					else
						SetFlavorSkill(controller.ChaControl.fileGameInfo, flavor, next);
			}
		}

		void Draw_Sliders_Desires()
		{
			if (controller.agent == null)
				return;

			GUILayout.BeginVertical(GUILayout.Width(ENTRY_WIDTH));
			{
				GUILayout.Label("Desires", sectionLabelStyle);

				Dictionary<int, float> desires = controller.agent.AgentData.DesireTable;

				foreach (KeyValuePair<Desire.Type, int> desire in desireTable)
				{
					int key = desire.Value;
					float curr = desires[key];
					float next = Draw_Contents_Slider(
						controller.lockedDesires,
						key,
						desire.Key.ToString(),
						curr,
						0f,
						100f
					);

					if (next != curr)
						desires[key] = next;
				}
			}
			GUILayout.EndVertical();
		}

		void Draw_Content_Sliders()
		{
			GUILayout.BeginVertical(GUILayout.Width(ENTRY_WIDTH));
			{
				Draw_Sliders_Stats();
				Draw_Sliders_Flavors();
			}
			GUILayout.EndVertical();

			Draw_Sliders_Desires();
		}

		float Draw_Info_Slider(string label,
							   float value,
							   float min,
							   float max,
							   Func<float, string> func = null)
		{
			GUILayout.BeginHorizontal();
			{
				label = $"{label}:\n";

				if (func != null)
					label += func(value);
				else
					label += value;

				GUILayout.Label(label, GUILayout.Width(LABEL_WIDTH));

				value = GUILayout.HorizontalSlider(value, min, max);
			}
			GUILayout.EndHorizontal();

			//GUILayout.Label(func != null ? func(value) : $"{value}", GUILayout.Width(LABEL_WIDTH));

			return value;
		}

		void Draw_Info_Sliders()
		{
			ChaFileGameInfo fileGameInfo = controller.ChaControl.fileGameInfo;
			Dictionary<int, string> lifestyles = Lifestyle.LifestyleName;

			GUILayout.BeginVertical(GUILayout.Width(ENTRY_WIDTH));
			{
				GUILayout.Label("Sliders", sectionLabelStyle);

				int phase = (int)Draw_Info_Slider(
					"Hearts",
					fileGameInfo.phase,
					0,
					3,
					v => (int)v + 1 + " Heart(s)"
				);

				if (phase != fileGameInfo.phase)
					fileGameInfo.phase = phase;

				int lifestyle = (int)Draw_Info_Slider(
					"Lifestyle",
					fileGameInfo.lifestyle,
					-1,
					lifestyles.Count - 1,
					v => (int)v == -1 ?
							"None" :
							lifestyles.ContainsKey((int)v) ?
								lifestyles[(int)v] :
								"Unknown"
				);

				if (lifestyle != fileGameInfo.lifestyle)
					fileGameInfo.lifestyle = lifestyle;

				int favoritePlace = (int)Draw_Info_Slider(
					"Favorite Place",
					fileGameInfo.favoritePlace,
					0,
					11
				);

				if (favoritePlace != fileGameInfo.favoritePlace)
					fileGameInfo.favoritePlace = favoritePlace;
			}
			GUILayout.EndVertical();
		}

		void Draw_Info_Texts()
		{
			if (controller.agent == null)
				return;

			AgentActor agent = controller.agent;
			PlayerActor player = Manager.Map.Instance.Player;
			Vector3 pos = controller.agent.Position;

			Sickness sick = agent.AgentData.SickState;
			string meds = sick.UsedMedicine ? "; Medicated" : "";
			double duration = sick.Duration.TotalSeconds;
			string time = duration > 0 ? $"; {duration.ToString()}s" : "";

			GUILayout.Label("Info", sectionLabelStyle);

			GUILayout.BeginVertical(GUILayout.Width(ENTRY_WIDTH));
			{
				GUILayout.Label($"Location:\n{(int)pos.x}, {(int)pos.y}, {(int)pos.z}");
				GUILayout.Label($"Distance to Player:\n{(int)Vector3.Distance(player.Position, pos)}");
				GUILayout.Label($"Speed:\n{agent.NavMeshAgent.speed}");
				GUILayout.Label($"Total Flavor:\n{controller.ChaControl.fileGameInfo.totalFlavor}");
				GUILayout.Label($"State:\n{agent.StateType.ToString()}");
				GUILayout.Label($"Desire:\n{agent.Mode.ToString()}");
				GUILayout.Label($"Sickness:\n{sick.Name}{time}{meds}");
			}
			GUILayout.EndVertical();
		}

		void Draw_Content_Info()
		{
			Draw_Info_Sliders();
			Draw_Info_Texts();
		}

		void Draw_Entries_Content()
		{
			switch (tab)
			{
				case 0:
					Draw_Content_Info();
					break;

				case 1:
					Draw_Content_Sliders();
					break;
			}
		}

		void Draw_Entries()
		{
			if (controller == null)
				return;

			Draw_Entries_Content();
		}

		void Draw_Tabs()
		{
			if (controller == null)
				return;

			GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
			{
				GUILayout.Label("Tabs", sectionLabelStyle);

				for (int i = 0; i < tabs.Count; i++)
					if (GUILayout.Button(
						tabs[i],
						i == tab ?
							selectedButtonStyle :
							GUI.skin.button
						))
					{
						tab = i;
						break;
					}
			}
			GUILayout.EndVertical();
		}

		void Draw_Agents()
		{
			GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
			{
				GUILayout.Label("Agents", sectionLabelStyle);

				foreach (StatsController controller in controllers)
				{
					if (controller.ChaControl == null)
					{
						if (this.controller == controller)
							this.controller = null;

						controllersDump.Add(controller);
						continue;
					}

					string label;

					if (CustomBase.IsInstance() &&
						CustomBase.Instance.chaCtrl == controller.ChaControl)
						label = "[New Agent]";
					else if (controller.agent != null)
						label = $"[{controller.id}]: {controller.agent.CharaName}";
					else
						label = $"[?]: {controller.ChaControl.chaFile.charaFileName}";

					GUIStyle style =
						this.controller == controller ? selectedButtonStyle : GUI.skin.button;

					if (GUILayout.Button(label, style, GUILayout.Width(AGENTS_WIDTH)))
						this.controller = controller;
				}
			}
			GUILayout.EndVertical();
		}

		void Draw(int id)
		{
			GUI.DragWindow(dragRect);
			GUILayout.BeginArea(innerRect);
			{
				GUILayout.BeginHorizontal();
				{
					GUILayout.BeginVertical(GUILayout.Width(AGENTS_WIDTH));
					{
						Draw_Agents();
						Draw_Tabs();
					}
					GUILayout.EndVertical();

					Draw_Entries();
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.EndArea();
		}
	}
}
