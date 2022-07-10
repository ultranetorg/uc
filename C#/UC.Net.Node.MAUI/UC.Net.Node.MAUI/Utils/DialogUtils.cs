namespace UC.Net.Node.MAUI
{
    public static class DialogUtils
    {
        public static async Task ShowPageAsDialog(this INavigation navigation, Page page)
        {
            int pagesOnStack = navigation.NavigationStack.Count + 1;
            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            page.Disappearing += (s, e) =>
            {
                if (navigation.NavigationStack.Count <= pagesOnStack)
                    waitHandle.Set();
            };
            await navigation.PushAsync(page);
            await Task.Run(() => waitHandle.WaitOne());
        }
        //public static async Task ShowPopupAsDialog(this INavigation navigation, PopupPage page)
        //{
        //    int pagesOnStack = navigation.NavigationStack.Count + 1;
        //    var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        //    page.Disappearing += (s, e) =>
        //    {
        //        if (navigation.NavigationStack.Count <= pagesOnStack)
        //            waitHandle.Set();
        //    };
        //    await navigation.PushPopupAsync(page);
        //    await Task.Run(() => waitHandle.WaitOne());
        //}
    }
}
