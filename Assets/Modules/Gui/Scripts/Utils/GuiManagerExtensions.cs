using Services.Gui;

namespace Gui.Utils
{
	public static class GuiManagerExtensions
	{
		public static void ShowConfirmationDialog(this IGuiManager guiManager, string message, System.Action action)
		{
			guiManager.OpenWindow(ConfirmationDialogName, new WindowArgs(message), result =>
			{
				if (result == WindowExitCode.Ok)
					action.Invoke();
			});
		}

		public static void ShowBuyConfirmationDialog(this IGuiManager guiManager, string message, Economy.Price price, System.Action action)
		{
			guiManager.OpenWindow(BuyConfirmationDialogName, new WindowArgs(message, price), result =>
			{
				if (result == WindowExitCode.Ok)
					action.Invoke();
			});
		}

		private const string ConfirmationDialogName = "ConfirmationDialog";
		private const string BuyConfirmationDialogName = "BuyConfirmationDialog";
	}
}
