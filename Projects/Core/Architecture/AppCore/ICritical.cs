namespace OnUtils.Architecture.AppCore
{
    /// <summary>
    /// Представляет компонент, являющийся критичным для запуска ядра. Так как <see cref="ICritical"/> наследует <see cref="IAutoStart"/>, то ядро предпримет попытку инициализировать экземпляр компонента во время запуска ядра. 
    /// Если во время создания или запуска компонента возникает исключение, то выбрасывается общее исключение <see cref="ApplicationStartException"/> со значением <see cref="ApplicationStartException.Step"/> равным <see cref="ApplicationStartStep.BindingsAutoStartCritical"/>.
    /// </summary>
    public interface ICritical : IAutoStart
    {
    }
}
