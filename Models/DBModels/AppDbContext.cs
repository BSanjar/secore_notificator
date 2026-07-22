using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NotificationWorker.Models.DBModels;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Agent> Agents { get; set; }

    public virtual DbSet<AgentCommission> AgentCommissions { get; set; }

    public virtual DbSet<Commission> Commissions { get; set; }

    public virtual DbSet<CommissionTier> CommissionTiers { get; set; }

    public virtual DbSet<History> Histories { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoicePayment> InvoicePayments { get; set; }

    public virtual DbSet<InvoiceService> InvoiceServices { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<OrgClientGroup> OrgClientGroups { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<OrganizationClient> OrganizationClients { get; set; }

    public virtual DbSet<OrganizationClientsAdditionalField> OrganizationClientsAdditionalFields { get; set; }

    public virtual DbSet<OrganizationField> OrganizationFields { get; set; }

    public virtual DbSet<OrganizationService> OrganizationServices { get; set; }

    public virtual DbSet<OrganizationSetting> OrganizationSettings { get; set; }

    public virtual DbSet<OrganizationSubscription> OrganizationSubscriptions { get; set; }

    public virtual DbSet<OrganizationSubscriptionPayment> OrganizationSubscriptionPayments { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("agent_pk");

            entity.ToTable("agent");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.Allowlistip)
                .HasColumnType("character varying")
                .HasColumnName("allowlistip");
            entity.Property(e => e.ApiLogin)
                .HasColumnType("character varying")
                .HasColumnName("api_login");
            entity.Property(e => e.ApiPsw)
                .HasColumnType("character varying")
                .HasColumnName("api_psw");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<AgentCommission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("agent_commission_pk");

            entity.ToTable("agent_commission", tb => tb.HasComment("Связь агента и организации с видом комиссии (верхняя от агента / нижняя к агенту)"));

            entity.HasIndex(e => new { e.AgentId, e.OrganizationId }, "agent_commission_agent_organization_uq").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.AgentId)
                .HasColumnType("character varying")
                .HasColumnName("agent_id");
            entity.Property(e => e.CommissionId)
                .HasColumnType("character varying")
                .HasColumnName("commission_id");
            entity.Property(e => e.LowerCommissionId)
                .HasColumnType("character varying")
                .HasColumnName("lower_commission_id");
            entity.Property(e => e.OrganizationId)
                .HasColumnType("character varying")
                .HasColumnName("organization_id");

            entity.HasOne(d => d.Agent).WithMany(p => p.AgentCommissions)
                .HasForeignKey(d => d.AgentId)
                .HasConstraintName("agent_commission_agent_fk");

            entity.HasOne(d => d.Commission).WithMany(p => p.AgentCommissionCommissions)
                .HasForeignKey(d => d.CommissionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("agent_commission_commission_fk");

            entity.HasOne(d => d.LowerCommission).WithMany(p => p.AgentCommissionLowerCommissions)
                .HasForeignKey(d => d.LowerCommissionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("agent_commission_lower_commission_fk");

            entity.HasOne(d => d.Organization).WithMany(p => p.AgentCommissions)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("agent_commission_organization_fk");
        });

        modelBuilder.Entity<Commission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("commission_pk");

            entity.ToTable("commission", tb => tb.HasComment("Справочник видов комиссии (верхняя/нижняя)"));

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.CommissionKind)
                .HasComment("percent | fixed | mixed | progressive")
                .HasColumnType("character varying")
                .HasColumnName("commission_kind");
            entity.Property(e => e.FixedAmount)
                .HasPrecision(18, 2)
                .HasColumnName("fixed_amount");
            entity.Property(e => e.MaxFee)
                .HasPrecision(18, 2)
                .HasColumnName("max_fee");
            entity.Property(e => e.MinFee)
                .HasPrecision(18, 2)
                .HasColumnName("min_fee");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Rate)
                .HasPrecision(18, 6)
                .HasColumnName("rate");
        });

        modelBuilder.Entity<CommissionTier>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("commission_tier_pk");

            entity.ToTable("commission_tier");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.AmountFrom)
                .HasPrecision(18, 2)
                .HasColumnName("amount_from");
            entity.Property(e => e.AmountTo)
                .HasPrecision(18, 2)
                .HasColumnName("amount_to");
            entity.Property(e => e.CommissionId)
                .HasColumnType("character varying")
                .HasColumnName("commission_id");
            entity.Property(e => e.Rate)
                .HasPrecision(18, 6)
                .HasColumnName("rate");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");

            entity.HasOne(d => d.Commission).WithMany(p => p.CommissionTiers)
                .HasForeignKey(d => d.CommissionId)
                .HasConstraintName("commission_tier_commission_fk");
        });

        modelBuilder.Entity<History>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("history");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.Invoice)
                .HasComment("если действие по инвойсу то id инвойса")
                .HasColumnType("character varying")
                .HasColumnName("invoice");
            entity.Property(e => e.TypeHistory)
                .HasComment("user_edited\r\ninvoice_edited")
                .HasColumnType("character varying")
                .HasColumnName("type_history");
            entity.Property(e => e.UserEditor)
                .HasComment("Пользователь который совершил действие")
                .HasColumnType("character varying")
                .HasColumnName("user_editor");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("invoice_pk");

            entity.ToTable("invoice");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.AutoProlongation)
                .HasDefaultValueSql("false")
                .HasComment("автопролонгация, если указан true - то при оплате за этот инвойс автоматом создается след запись в табилице графика платежей")
                .HasColumnName("auto_prolongation");
            entity.Property(e => e.Balance)
                .HasComment("сумма баланса, если сумма в минусе то долг.\r\nСумма указывается в тыйынах")
                .HasColumnName("balance");
            entity.Property(e => e.Client)
                .HasColumnType("character varying")
                .HasColumnName("client");
            entity.Property(e => e.DateCreated)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_created");
            entity.Property(e => e.DateEndInvoice)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_end_invoice");
            entity.Property(e => e.DateStartInvoice)
                .HasComment("Дата начал инвойса, т.е с этого дня будет учитываться платеж")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_start_invoice");
            entity.Property(e => e.FixedSumm)
                .HasComment("фиксированная сумма платежа, если не указан или 0 то сумма для платежа любая сумма")
                .HasColumnName("fixed_summ");
            entity.Property(e => e.Hassameaccount)
                .HasComment("если true - то это есть еще другой счет с таким же лицевым счетом и у которых общий баланс")
                .HasColumnName("hassameaccount");
            entity.Property(e => e.InvoiceStatus)
                .HasComment("actual\r\nsuspended\r\nclosed")
                .HasColumnType("character varying")
                .HasColumnName("invoice_status");
            entity.Property(e => e.NameInvoice)
                .HasColumnType("character varying")
                .HasColumnName("name_invoice");
            entity.Property(e => e.NextStartInvoice)
                .HasComment("Дата начала следующего периода инвойса, если автопролонгация или инвойс установлен на несколько периодов")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("next_start_invoice");
            entity.Property(e => e.PayCode)
                .HasColumnType("character varying")
                .HasColumnName("pay_code");
            entity.Property(e => e.Periodicity)
                .HasComment("периодичность оплаты:\r\ndaily - ежедневно\r\nweekly - еженедельно\r\nmonthly - ежемесячно\r\nyearly - ежегодно\r\nесли указывается конкретное число то значит каждые указанное число дней. \r\nт.е если к примеру 30 то каждые 30 дней.\r\noneTime - однаразовый и прием в любой момент\r\nany - прием в любой момент")
                .HasColumnType("character varying")
                .HasColumnName("periodicity");
            entity.Property(e => e.UserCreater)
                .HasColumnType("character varying")
                .HasColumnName("user_creater");

            entity.HasOne(d => d.ClientNavigation).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.Client)
                .HasConstraintName("invoice_fk_1");

            entity.HasOne(d => d.UserCreaterNavigation).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.UserCreater)
                .HasConstraintName("invoice_fk");
        });

        modelBuilder.Entity<InvoicePayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("invoice_payments_pk");

            entity.ToTable("invoice_payments", tb => tb.HasComment("записи - за какие периоды оплачены"));

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.DateFrom)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_from");
            entity.Property(e => e.DateTo)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_to");
            entity.Property(e => e.Invoice)
                .HasColumnType("character varying")
                .HasColumnName("invoice");
            entity.Property(e => e.PaymentStatus)
                .HasComment("paid - уже оплатил\r\nnon_paid - еще не оплатил\r\nanulated - в таком случае д\\с возвращается обратно на баланс по инвойсу")
                .HasColumnType("character varying")
                .HasColumnName("payment_status");
            entity.Property(e => e.PaymentSumm)
                .HasComment("сумма оплаты")
                .HasColumnName("payment_summ");
            entity.Property(e => e.PeriodValue)
                .HasComment("какой месяц или год\r\nесли периодичность месяц или год")
                .HasColumnType("character varying")
                .HasColumnName("period_value");

            entity.HasOne(d => d.InvoiceNavigation).WithMany(p => p.InvoicePayments)
                .HasForeignKey(d => d.Invoice)
                .HasConstraintName("invoice_payments_fk");
        });

        modelBuilder.Entity<InvoiceService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("invoice_services_pk");

            entity.ToTable("invoice_services");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.Invoice)
                .HasColumnType("character varying")
                .HasColumnName("invoice");
            entity.Property(e => e.Service)
                .HasColumnType("character varying")
                .HasColumnName("service");
            entity.Property(e => e.ServiceSumm)
                .HasComment("стоимость сервиса")
                .HasColumnName("service_summ");

            entity.HasOne(d => d.InvoiceNavigation).WithMany(p => p.InvoiceServices)
                .HasForeignKey(d => d.Invoice)
                .HasConstraintName("invoice_services_fk");

            entity.HasOne(d => d.ServiceNavigation).WithMany(p => p.InvoiceServices)
                .HasForeignKey(d => d.Service)
                .HasConstraintName("invoice_services_fk_1");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            // Схема как в основном secore / InitialCreate (без invoice_id, organization_id и т.п.)
            entity.HasKey(e => e.Id).HasName("notifications_pk");

            entity.ToTable("notifications");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.Channel)
                .HasComment("email, telegram, whatsapp")
                .HasColumnType("character varying")
                .HasColumnName("channel");
            entity.Property(e => e.ClientId)
                .HasColumnType("character varying")
                .HasColumnName("client_id");
            entity.Property(e => e.ContactInfo)
                .HasComment("email, телефон, telegram chat_id")
                .HasColumnType("character varying")
                .HasColumnName("contact_info");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ErrorMessage)
                .HasColumnType("text")
                .HasColumnName("error_message");
            entity.Property(e => e.Message)
                .HasColumnType("text")
                .HasColumnName("message");
            entity.Property(e => e.Metadata)
                .HasColumnType("text")
                .HasColumnName("metadata");
            entity.Property(e => e.ProcessedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("processed_at");
            entity.Property(e => e.RetryCount)
                .HasDefaultValueSql("0")
                .HasColumnName("retry_count");
            entity.Property(e => e.SentAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sent_at");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'new'")
                .HasComment("new - новое, ожидает обработки\r\nprocessing - в процессе обработки\r\nsent - отправлено\r\nfailed - ошибка отправки")
                .HasColumnType("character varying")
                .HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasColumnType("character varying")
                .HasColumnName("subject");

            entity.HasOne(d => d.Client).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("notifications_fk");
        });

        modelBuilder.Entity<OrgClientGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("org_client_groups_pk");

            entity.ToTable("org_client_groups");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.IsDeleted)
                .HasComment("0 — активна, 1 — удалена (мягкое удаление)")
                .HasColumnName("is_deleted");
            entity.Property(e => e.Logo)
                .HasColumnType("character varying")
                .HasColumnName("logo");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.OrganizationId)
                .HasColumnType("character varying")
                .HasColumnName("organization_id");
            entity.Property(e => e.ParentGroupId)
                .HasColumnType("character varying")
                .HasColumnName("parent_group_id");

            entity.HasOne(d => d.Organization).WithMany(p => p.OrgClientGroups)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("org_client_groups_organization_fk");

            entity.HasOne(d => d.ParentGroup).WithMany(p => p.InverseParentGroup)
                .HasForeignKey(d => d.ParentGroupId)
                .HasConstraintName("org_client_groups_parent_fk");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("organization_pk");

            entity.ToTable("organization");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasComment("false — доступ заблокирован (истёк период подписки)")
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Organizationtype)
                .HasComment("standart\r\ndetsad\r\nschool\r\nmedclinic")
                .HasColumnType("character varying")
                .HasColumnName("organizationtype");
        });

        modelBuilder.Entity<OrganizationClient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("organization_cients_pk");

            entity.ToTable("organization_clients");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.ClientAddress)
                .HasColumnType("character varying")
                .HasColumnName("client_address");
            entity.Property(e => e.ClientBalance)
                .HasComment("баланс в тыйынах")
                .HasColumnName("client_balance");
            entity.Property(e => e.ClientEmail)
                .HasColumnType("character varying")
                .HasColumnName("client_email");
            entity.Property(e => e.ClientInn)
                .HasColumnType("character varying")
                .HasColumnName("client_inn");
            entity.Property(e => e.ClientLogo)
                .HasColumnType("character varying")
                .HasColumnName("client_logo");
            entity.Property(e => e.ClientName)
                .HasColumnType("character varying")
                .HasColumnName("client_name");
            entity.Property(e => e.ClientPhone)
                .HasColumnType("character varying")
                .HasColumnName("client_phone");
            entity.Property(e => e.ClientStatus)
                .HasDefaultValueSql("0")
                .HasComment("0\\1")
                .HasColumnName("client_status");
            entity.Property(e => e.ClientTg)
                .HasComment("телеграмм клиента")
                .HasColumnType("character varying")
                .HasColumnName("client_tg");
            entity.Property(e => e.ClientType)
                .HasComment("fiz\\jur")
                .HasColumnType("character varying")
                .HasColumnName("client_type");
            entity.Property(e => e.ClientWa)
                .HasComment("ватсап клиента")
                .HasColumnType("character varying")
                .HasColumnName("client_wa");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.OrgClientGroupId)
                .HasColumnType("character varying")
                .HasColumnName("org_client_group_id");
            entity.Property(e => e.Organization)
                .HasColumnType("character varying")
                .HasColumnName("organization");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_date");
            entity.Property(e => e.UserCreater)
                .HasColumnType("character varying")
                .HasColumnName("user_creater");

            entity.HasOne(d => d.OrgClientGroup).WithMany(p => p.OrganizationClients)
                .HasForeignKey(d => d.OrgClientGroupId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("organization_clients_org_client_group_fk");

            entity.HasOne(d => d.OrganizationNavigation).WithMany(p => p.OrganizationClients)
                .HasForeignKey(d => d.Organization)
                .HasConstraintName("organization_clients_fk");

            entity.HasOne(d => d.UserCreaterNavigation).WithMany(p => p.OrganizationClients)
                .HasForeignKey(d => d.UserCreater)
                .HasConstraintName("organization_clients_fk2");
        });

        modelBuilder.Entity<OrganizationClientsAdditionalField>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("organization_clients_additional_fields_pk");

            entity.ToTable("organization_clients_additional_fields");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.Field)
                .HasComment("ссылка на organization_fields")
                .HasColumnType("character varying")
                .HasColumnName("field");
            entity.Property(e => e.OrganizationClient)
                .HasComment("ссылка на клиента")
                .HasColumnType("character varying")
                .HasColumnName("organization_client");
            entity.Property(e => e.Value)
                .HasComment("значение переменной\\поля")
                .HasColumnType("character varying")
                .HasColumnName("value");

            entity.HasOne(d => d.FieldNavigation).WithMany(p => p.OrganizationClientsAdditionalFields)
                .HasForeignKey(d => d.Field)
                .HasConstraintName("organization_clients_additional_fields_fk");

            entity.HasOne(d => d.OrganizationClientNavigation).WithMany(p => p.OrganizationClientsAdditionalFields)
                .HasForeignKey(d => d.OrganizationClient)
                .HasConstraintName("organization_clients_additional_fields_fk_1");
        });

        modelBuilder.Entity<OrganizationField>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("organization_fields_pk");

            entity.ToTable("organization_fields");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.FieldName)
                .HasColumnType("character varying")
                .HasColumnName("field_name");
            entity.Property(e => e.FieldSelectValues)
                .HasComment("варианты для выбора чз - ;")
                .HasColumnType("character varying")
                .HasColumnName("field_select_values");
            entity.Property(e => e.FieldType)
                .HasComment("int\r\nstring\r\nmoney\r\nselected\r\ndatetime")
                .HasColumnType("character varying")
                .HasColumnName("field_type");
            entity.Property(e => e.Filterbyfield)
                .HasDefaultValueSql("false")
                .HasColumnName("filterbyfield");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("0")
                .HasColumnName("isdeleted");
            entity.Property(e => e.Organization)
                .HasColumnType("character varying")
                .HasColumnName("organization");

            entity.HasOne(d => d.OrganizationNavigation).WithMany(p => p.OrganizationFields)
                .HasForeignKey(d => d.Organization)
                .HasConstraintName("organization_fields_fk");
        });

        modelBuilder.Entity<OrganizationService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("organization_services_pk");

            entity.ToTable("organization_services");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.FixedSum)
                .HasDefaultValueSql("0")
                .HasComment("если 1 то услуга с фиксированной суммой")
                .HasColumnName("fixed_sum");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("0")
                .HasColumnName("isdeleted");
            entity.Property(e => e.MaxSumm)
                .HasComment("макс сумма в тыйынах")
                .HasColumnName("max_summ");
            entity.Property(e => e.MinSumm)
                .HasComment("мин сумма в тыйынах")
                .HasColumnName("min_summ");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Organization)
                .HasColumnType("character varying")
                .HasColumnName("organization");
            entity.Property(e => e.ServiceSumm)
                .HasComment("если fixed_sum = 1, то тут будет значение фиксированной суммы")
                .HasColumnName("service_summ");

            entity.HasOne(d => d.OrganizationNavigation).WithMany(p => p.OrganizationServices)
                .HasForeignKey(d => d.Organization)
                .HasConstraintName("organization_services_fk");
        });

        modelBuilder.Entity<OrganizationSetting>(entity =>
        {
            entity.HasKey(e => e.OrganizationId).HasName("organization_settings_pk");

            entity.ToTable("organization_settings", tb => tb.HasComment("Настройки организации (1:1 с organization)"));

            entity.Property(e => e.OrganizationId)
                .HasComment("PK и FK на organization.id")
                .HasColumnType("character varying")
                .HasColumnName("organization_id");
            entity.Property(e => e.Address)
                .HasColumnType("character varying")
                .HasColumnName("address");
            entity.Property(e => e.AllowedHassameaccount)
                .HasComment("если true - организации могут создавать счета с одинаковыми л/с")
                .HasColumnName("allowed_hassameaccount");
            entity.Property(e => e.BillingType)
                .HasComment("subscription или комбинация комиссий")
                .HasColumnType("character varying")
                .HasColumnName("billing_type");
            entity.Property(e => e.CommissionId)
                .HasComment("FK на commission.id для нижней от организации")
                .HasColumnType("character varying")
                .HasColumnName("commission_id");
            entity.Property(e => e.ContactPhone)
                .HasColumnType("character varying")
                .HasColumnName("contact_phone");
            entity.Property(e => e.DirectorFullName)
                .HasColumnType("character varying")
                .HasColumnName("director_full_name");
            entity.Property(e => e.DisableInvoiceServiceSelection)
                .HasComment("отключить выбор услуги при создании счёта; ввод названия и цены вручную")
                .HasColumnName("disable_invoice_service_selection");
            entity.Property(e => e.Email)
                .HasColumnType("character varying")
                .HasColumnName("email");
            entity.Property(e => e.InvoicePayCodeMode)
                .HasMaxLength(32)
                .HasComment("new_only = только новый л/с; duplicate_only = только существующий; both = новый и существующий")
                .HasColumnName("invoice_pay_code_mode");
            entity.Property(e => e.LogoPath)
                .HasColumnType("character varying")
                .HasColumnName("logo_path");
            entity.Property(e => e.Paymentreminderdaysbefore)
                .HasComment("за сколько дней до срока начинать напоминания по оплате")
                .HasColumnName("paymentreminderdaysbefore");
            entity.Property(e => e.UseLowerCommissionFromOrg)
                .HasComment("нижняя комиссия от организации (от оборота)")
                .HasColumnName("use_lower_commission_from_org");
            entity.Property(e => e.UseLowerCommissionToAgent)
                .HasComment("нижняя комиссия к агенту")
                .HasColumnName("use_lower_commission_to_agent");
            entity.Property(e => e.UseUpperCommissionFromAgent)
                .HasComment("верхняя комиссия от агента")
                .HasColumnName("use_upper_commission_from_agent");
            entity.Property(e => e.WhatsappPhone)
                .HasColumnType("character varying")
                .HasColumnName("whatsapp_phone");

            entity.HasOne(d => d.Organization).WithOne(p => p.OrganizationSetting)
                .HasForeignKey<OrganizationSetting>(d => d.OrganizationId)
                .HasConstraintName("organization_settings_organization_fk");
        });

        modelBuilder.Entity<OrganizationSubscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("organization_subscription_pkey");

            entity.ToTable("organization_subscription");

            entity.HasIndex(e => e.OrganizationId, "organization_subscription_organization_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.OrganizationId)
                .HasColumnType("character varying")
                .HasColumnName("organization_id");
            entity.Property(e => e.PeriodType)
                .HasDefaultValueSql("'month'::character varying")
                .HasColumnType("character varying")
                .HasColumnName("period_type");
            entity.Property(e => e.PriceTyiyn)
                .HasPrecision(18, 2)
                .HasColumnName("price_tyiyn");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Organization).WithOne(p => p.OrganizationSubscription)
                .HasForeignKey<OrganizationSubscription>(d => d.OrganizationId)
                .HasConstraintName("organization_subscription_organization_id_fkey");
        });

        modelBuilder.Entity<OrganizationSubscriptionPayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("organization_subscription_payment_pkey");

            entity.ToTable("organization_subscription_payment");

            entity.HasIndex(e => e.OrganizationId, "ix_organization_subscription_payment_organization_id");

            entity.HasIndex(e => new { e.OrganizationId, e.PeriodStart, e.PeriodEnd }, "ix_organization_subscription_payment_period");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.AmountTyiyn)
                .HasPrecision(18, 2)
                .HasColumnName("amount_tyiyn");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Note)
                .HasColumnType("character varying")
                .HasColumnName("note");
            entity.Property(e => e.OrganizationId)
                .HasColumnType("character varying")
                .HasColumnName("organization_id");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end");
            entity.Property(e => e.PeriodStart).HasColumnName("period_start");

            entity.HasOne(d => d.Organization).WithMany(p => p.OrganizationSubscriptionPayments)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("organization_subscription_payment_organization_id_fkey");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("permissions_pkey");

            entity.ToTable("permissions");

            entity.HasIndex(e => e.Area, "idx_permissions_area");

            entity.HasIndex(e => e.Category, "idx_permissions_category");

            entity.HasIndex(e => e.Code, "idx_permissions_code");

            entity.HasIndex(e => e.Isdeleted, "idx_permissions_isdeleted");

            entity.HasIndex(e => e.Code, "permissions_code_key").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.Area)
                .HasColumnType("character varying")
                .HasColumnName("area");
            entity.Property(e => e.Category)
                .HasColumnType("character varying")
                .HasColumnName("category");
            entity.Property(e => e.Code)
                .HasColumnType("character varying")
                .HasColumnName("code");
            entity.Property(e => e.Description)
                .HasColumnType("character varying")
                .HasColumnName("description");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("0")
                .HasColumnName("isdeleted");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pk");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Organization, "idx_roles_organization");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.AvilableServices)
                .HasComment("перечисляется id сервисов чз ;")
                .HasColumnType("character varying")
                .HasColumnName("avilable_services");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("0")
                .HasColumnName("isdeleted");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Organization)
                .HasColumnType("character varying")
                .HasColumnName("organization");
            entity.Property(e => e.Rights)
                .HasComment("права пользователей")
                .HasColumnType("character varying")
                .HasColumnName("rights");

            entity.HasOne(d => d.OrganizationNavigation).WithMany(p => p.Roles)
                .HasForeignKey(d => d.Organization)
                .HasConstraintName("roles_fk_organization");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("role_permissions_pkey");

            entity.ToTable("role_permissions");

            entity.HasIndex(e => e.Isdeleted, "idx_role_permissions_isdeleted");

            entity.HasIndex(e => e.Permission, "idx_role_permissions_permission");

            entity.HasIndex(e => e.Role, "idx_role_permissions_role");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("0")
                .HasColumnName("isdeleted");
            entity.Property(e => e.Permission)
                .HasColumnType("character varying")
                .HasColumnName("permission");
            entity.Property(e => e.Role)
                .HasColumnType("character varying")
                .HasColumnName("role");

            entity.HasOne(d => d.PermissionNavigation).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.Permission)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("role_permissions_fk_1");

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.Role)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("role_permissions_fk");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("transactions_pk");

            entity.ToTable("transactions");

            entity.Property(e => e.Id)
                .HasComment("avn_txn_id")
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.Agent)
                .HasComment("если это оплата по API то с какого конкретно агента, id агента.")
                .HasColumnType("character varying")
                .HasColumnName("agent");
            entity.Property(e => e.Invoice)
                .HasColumnType("character varying")
                .HasColumnName("invoice");
            entity.Property(e => e.LowerCommissionFromOrg)
                .HasPrecision(18, 2)
                .HasComment("Нижняя комиссия от организации (тыйыны)")
                .HasColumnName("lower_commission_from_org");
            entity.Property(e => e.LowerCommissionToAgent)
                .HasPrecision(18, 2)
                .HasComment("Нижняя комиссия к агенту (тыйыны)")
                .HasColumnName("lower_commission_to_agent");
            entity.Property(e => e.PaymentInvoice)
                .HasComment("если транзакция внутренняя и по конкретному графику, то ссылка на график")
                .HasColumnType("character varying")
                .HasColumnName("payment_invoice");
            entity.Property(e => e.Summ)
                .HasComment("сумма в тыйынах")
                .HasColumnName("summ");
            entity.Property(e => e.TransactionDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("transaction_date");
            entity.Property(e => e.TransactionStatus)
                .HasComment("success\r\nerror")
                .HasColumnType("character varying")
                .HasColumnName("transaction_status");
            entity.Property(e => e.TransactionSumm)
                .HasComment("сумма транзакции в тыйынах вмесе с комиссией")
                .HasColumnName("transaction_summ");
            entity.Property(e => e.TransactionSystem)
                .HasComment("в какой системе происходила транзакция:\r\n-secore\r\n-secorePaymentSheduler")
                .HasColumnType("character varying")
                .HasColumnName("transaction_system");
            entity.Property(e => e.TransactionType)
                .HasComment("payFromAPI - оплата через API коннектор (приход в организацию)\r\ncredit - расход ранее оплаченных сумм. \r\n(возврат денег обратно клиенту итд).\r\npayPaymentInvoice - оплата конкретного счета инвойса, внутренняя операция.")
                .HasColumnType("character varying")
                .HasColumnName("transaction_type");
            entity.Property(e => e.TxnId)
                .HasComment("txnid платежа из запроса\r\n(txnid - уникальное значение в рамках одной организации)")
                .HasColumnType("character varying")
                .HasColumnName("txn_id");
            entity.Property(e => e.UpperCommissionFromAgent)
                .HasPrecision(18, 2)
                .HasComment("Верхняя комиссия от агента (тыйыны)")
                .HasColumnName("upper_commission_from_agent");

            entity.HasOne(d => d.AgentNavigation).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.Agent)
                .HasConstraintName("transactions_fk2");

            entity.HasOne(d => d.InvoiceNavigation).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.Invoice)
                .HasConstraintName("transactions_fk");

            entity.HasOne(d => d.PaymentInvoiceNavigation).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PaymentInvoice)
                .HasConstraintName("transactions_fk3");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pk");

            entity.ToTable("users");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.AccessFailedCount)
                .HasDefaultValueSql("0")
                .HasColumnName("access_failed_count");
            entity.Property(e => e.Email)
                .HasColumnType("character varying")
                .HasColumnName("email");
            entity.Property(e => e.EmailConfirmed)
                .HasDefaultValueSql("false")
                .HasColumnName("email_confirmed");
            entity.Property(e => e.GoogleEmail)
                .HasColumnType("character varying")
                .HasColumnName("google_email");
            entity.Property(e => e.GoogleId)
                .HasColumnType("character varying")
                .HasColumnName("google_id");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("0")
                .HasColumnName("isdeleted");
            entity.Property(e => e.LastLogin)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_login");
            entity.Property(e => e.LockoutEnd)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("lockout_end");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Organization)
                .HasColumnType("character varying")
                .HasColumnName("organization");
            entity.Property(e => e.Password)
                .HasColumnType("character varying")
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasColumnType("character varying")
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasComment("user\r\nadmin\r\nsuperadmin")
                .HasColumnType("character varying")
                .HasColumnName("role");
            entity.Property(e => e.TwoFactorCode)
                .HasColumnType("character varying")
                .HasColumnName("two_factor_code");
            entity.Property(e => e.TwoFactorCodeExpire)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("two_factor_code_expire");
            entity.Property(e => e.TwoFactorEnabled)
                .HasDefaultValueSql("false")
                .HasColumnName("two_factor_enabled");
            entity.Property(e => e.TwoFactorSecret)
                .HasColumnType("character varying")
                .HasColumnName("two_factor_secret");
            entity.Property(e => e.TwoFactorType)
                .HasColumnType("character varying")
                .HasColumnName("two_factor_type");

            entity.HasOne(d => d.OrganizationNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.Organization)
                .HasConstraintName("users_fk");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_roles_pk");

            entity.ToTable("user_roles");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.Isdeleted)
                .HasDefaultValueSql("0")
                .HasColumnName("isdeleted");
            entity.Property(e => e.Role)
                .HasColumnType("character varying")
                .HasColumnName("role");
            entity.Property(e => e.User)
                .HasColumnType("character varying")
                .HasColumnName("user");

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.Role)
                .HasConstraintName("user_roles_fk_1");

            entity.HasOne(d => d.UserNavigation).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.User)
                .HasConstraintName("user_roles_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
