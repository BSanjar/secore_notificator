using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NotificationWorker.Models.DBModels;

[Table("organization_clients")]
public partial class OrganizationClient
{
    [Key]
    [Column("id", TypeName = "character varying")]
    public string Id { get; set; } = null!;

    [Column("organization", TypeName = "character varying")]
    public string? Organization { get; set; }

    [Column("client_name", TypeName = "character varying")]
    public string? ClientName { get; set; }

    [Column("clinet_type", TypeName = "character varying")]
    public string? ClinetType { get; set; }

    [Column("client_inn", TypeName = "character varying")]
    public string? ClientInn { get; set; }

    [Column("client_phone", TypeName = "character varying")]
    public string? ClientPhone { get; set; }

    [Column("client_adres", TypeName = "character varying")]
    public string? ClientAdres { get; set; }

    [Column("client_email", TypeName = "character varying")]
    public string? ClientEmail { get; set; }

    [Column("client_balance")]
    public decimal? ClientBalance { get; set; }

    [Column("client_status")]
    public int? ClientStatus { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime? CreatedDate { get; set; }

    [Column("updated_date", TypeName = "timestamp without time zone")]
    public DateTime? UpdatedDate { get; set; }

    [Column("user_creater", TypeName = "character varying")]
    public string? UserCreater { get; set; }

    [Column("client_logo", TypeName = "character varying")]
    public string? ClientLogo { get; set; }

    [InverseProperty("ClientNavigation")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

