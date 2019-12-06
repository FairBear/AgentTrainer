using AIChara;
using AIProject;
using CharaCustom;
using System.Collections.Generic;
using UnityEngine;

namespace AgentTrainer
{
	public partial class AgentTrainer
	{
		public static HashSet<StatsController> controllersQueue = new HashSet<StatsController>();
		public static HashSet<StatsController> controllers = new HashSet<StatsController>();
		public static HashSet<StatsController> controllersDump = new HashSet<StatsController>();

		public static void AddController(StatsController controller)
		{
			if (!controllers.Contains(controller))
				controllersQueue.Add(controller);
		}

		public static void UpdateControllers()
		{
			foreach (StatsController controller in controllersQueue)
			{
				if (CustomBase.IsInstance() && CustomBase.Instance.chaCtrl == controller.ChaControl)
					controllers.Add(controller);
				else
					foreach (KeyValuePair<int, AgentActor> agent in Manager.Map.Instance.AgentTable)
						if (agent.Value.ChaControl == controller.ChaControl)
						{
							controller.id = agent.Key;
							controller.agent = agent.Value;

							controllers.Add(controller);
							break;
						}
			}

			controllersQueue.Clear();

			foreach (StatsController controller in controllersDump)
				controllers.Remove(controller);

			controllersDump.Clear();
		}

		public static void SetFlavorSkill(ChaFileGameInfo fileGameInfo, int id, int value)
		{
			int num = value - fileGameInfo.flavorState[id];
			fileGameInfo.flavorState[id] = Mathf.Clamp(value, 0, 99999);

			if (id == 4)
			{
				if (!fileGameInfo.isHAddTaii0 && fileGameInfo.flavorState[id] >= 100)
				{
					fileGameInfo.isHAddTaii0 = true;
				}
				if (!fileGameInfo.isHAddTaii1 && fileGameInfo.flavorState[id] >= 170)
				{
					fileGameInfo.isHAddTaii1 = true;
				}
			}

			int a = fileGameInfo.totalFlavor + num;
			fileGameInfo.totalFlavor = Mathf.Max(a, 0);
		}
	}
}
