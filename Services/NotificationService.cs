#if ANDROID
using Android.App;
using Android.Content;
using AndroidX.Core.App;
#endif

namespace MonBudget.Services;

/// <summary>
/// Notifications système. Android : vraie notification (avec demande d'autorisation sur Android 13+).
/// Autres plateformes : retourne false, l'application affiche alors une alerte à l'écran.
/// </summary>
public class NotificationService : INotificationService
{
#if ANDROID
    private const string CanalId = "alertes_budget";
    private static int _compteur = 1000;

    // Autorisation d'envoyer des notifications (obligatoire à partir d'Android 13)
    private sealed class PermissionNotifications : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new[] { (global::Android.Manifest.Permission.PostNotifications, true) };
    }
#endif

    public async Task<bool> NotifierAsync(string titre, string message)
    {
#if ANDROID
        try
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                var statut = await Permissions.CheckStatusAsync<PermissionNotifications>();
                if (statut != PermissionStatus.Granted)
                    statut = await MainThread.InvokeOnMainThreadAsync(() => Permissions.RequestAsync<PermissionNotifications>());
                if (statut != PermissionStatus.Granted) return false;
            }

            var contexte = global::Android.App.Application.Context;

            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                var canal = new NotificationChannel(CanalId, "Alertes budget", NotificationImportance.High)
                {
                    Description = "Alertes quand le budget mensuel est presque atteint ou dépassé"
                };
                var gestionnaire = contexte.GetSystemService(Context.NotificationService) as NotificationManager;
                gestionnaire?.CreateNotificationChannel(canal);
            }

            // Un appui sur la notification rouvre l'application
            var intention = new Intent(contexte, typeof(MainActivity));
            intention.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);
            var pending = PendingIntent.GetActivity(contexte, 0, intention,
                PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

            var notification = new NotificationCompat.Builder(contexte, CanalId)
                .SetSmallIcon(global::Android.Resource.Drawable.IcDialogInfo)
                .SetContentTitle(titre)
                .SetContentText(message)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(message))
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetAutoCancel(true)
                .SetContentIntent(pending)
                .Build();

            NotificationManagerCompat.From(contexte).Notify(_compteur++, notification);
            return true;
        }
        catch
        {
            return false;
        }
#else
        await Task.CompletedTask;
        return false;
#endif
    }
}
