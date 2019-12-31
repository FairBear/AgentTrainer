using AIChara;
using CharaCustom;
using HarmonyLib;
using System;
using UnityEngine;

namespace AgentTrainer
{
	public partial class AgentTrainer
	{
		[HarmonyPostfix, HarmonyPatch(typeof(CvsO_CharaLoad), "Start")]
		public static void Postfix_CvsO_CharaLoad_Start(CvsO_CharaLoad __instance, ref CustomCharaWindow ___charaLoadWin)
		{
			Action<CustomCharaFileInfo, int> act = ___charaLoadWin.onClick03;

			___charaLoadWin.onClick03 = (info, flags) =>
			{
				act(info, flags);

				CustomBase customBase = CustomBase.Instance;
				ChaControl chaCtrl = customBase.chaCtrl;

				if (0 != (flags & 16) && customBase.modeNew)
				{
					ChaFileControl dummy = new ChaFileControl();

					if (dummy.LoadCharaFile(info.FullPath, chaCtrl.sex, true, true))
						chaCtrl.fileGameInfo.Copy(dummy.gameinfo);
				}
			};
		}

		[HarmonyPrefix, HarmonyPatch(typeof(CvsO_CharaSave), "UpdateCharasList")]
		public static bool Prefix_CvsO_CharaSave_UpdateCharasList(ref CustomCharaWindow ___charaLoadWin)
		{
			CustomBase customBase = CustomBase.Instance;

			___charaLoadWin.UpdateWindow(customBase.modeNew, customBase.modeSex, false);

			return false;
		}
	}
}
