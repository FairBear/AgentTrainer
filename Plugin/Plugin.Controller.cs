using Manager;

namespace AgentTrainer
{
	public partial class AgentTrainer
	{
		StatsController controller;

		void Update()
		{
			if (Key.Value.IsDown())
				visible = !visible;

			if (Map.IsInstance())
				UpdateControllers();
		}
	}
}
