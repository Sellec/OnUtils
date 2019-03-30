using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnUtils.Architecture.AppCore
{
    /// <summary>
    /// Для singlton-компонентов ядра, имеющих привязку в DI ядра и реализующих данный интерфейс, автоматически создаются экземпляры при применении привязок <see cref="AppCore{TAppCore}.BindingsApply(DI.BindingsCollection{TAppCore})"/>.
    /// Этот интерфейс должен применяться к queryType.
    /// </summary>
    public interface IAutoStart
    {
    }
}
