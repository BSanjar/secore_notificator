using System;
using System.Collections.Generic;

namespace NotificationWorker.Models.DBModels;

public partial class History
{
    public string Id { get; set; } = null!;

    /// <summary>
    /// user_edited
    /// invoice_edited
    /// </summary>
    public string? TypeHistory { get; set; }

    /// <summary>
    /// Пользователь который совершил действие
    /// </summary>
    public string? UserEditor { get; set; }

    /// <summary>
    /// если действие по инвойсу то id инвойса
    /// </summary>
    public string? Invoice { get; set; }
}
