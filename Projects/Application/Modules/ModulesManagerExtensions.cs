namespace OnUtils.Application
{
    using Modules;

    /// <summary>
    /// Расширения для менеджера модулей. Учитывая, что менеджер модулей для веб-ядра не может наследоваться ни от чего, кроме как от <see cref="ModulesManager"/> и регистрироваться только через SetSingleton{ModulesManager{ApplicationCore}, ModulesManager}, ошибок InvalidCastException не должно возникать.
    /// </summary>
    public static class ModulesManagerExtensions
    {

    }
}
