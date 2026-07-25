namespace BMPharma.Shared.Constants;

public static class PermissionConstants
{
    public static class Products
    {
        public const string View = "products.view";
        public const string Create = "products.create";
        public const string Edit = "products.edit";
        public const string Delete = "products.delete";
        public const string Import = "products.import";
        public const string Export = "products.export";
    }

    public static class Stock
    {
        public const string View = "stock.view";
        public const string Manage = "stock.manage";
        public const string Adjust = "stock.adjust";
        public const string Transfer = "stock.transfer";
    }

    public static class Invoices
    {
        public const string View = "invoices.view";
        public const string Create = "invoices.create";
        public const string Edit = "invoices.edit";
        public const string Delete = "invoices.delete";
        public const string Print = "invoices.print";
        public const string Refund = "invoices.refund";
    }

    public static class Customers
    {
        public const string View = "customers.view";
        public const string Create = "customers.create";
        public const string Edit = "customers.edit";
        public const string Delete = "customers.delete";
    }

    public static class Bordereau
    {
        public const string View = "bordereau.view";
        public const string Create = "bordereau.create";
        public const string Sign = "bordereau.sign";
        public const string Close = "bordereau.close";
    }

    public static class Reports
    {
        public const string View = "reports.view";
        public const string Export = "reports.export";
    }

    public static class Settings
    {
        public const string View = "settings.view";
        public const string Edit = "settings.edit";
    }

    public static class Users
    {
        public const string View = "users.view";
        public const string Create = "users.create";
        public const string Edit = "users.edit";
        public const string Delete = "users.delete";
    }

    public static class License
    {
        public const string View = "license.view";
        public const string Activate = "license.activate";
    }
}
