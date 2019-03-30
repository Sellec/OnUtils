using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceStudio.Utils.Architecture.LogicalCore
{
    /// <summary>
    /// Коды ответа сервера в ответе на запрос к операции.
    /// </summary>
    public enum ResponseReturnCode : int
    {
        /// <summary>
        /// Код ответа отсутствует.
        /// </summary>
        None = 1,

        /// <summary>
        /// Указывает, что сервер не смог найти подходящий тип, реализующий запрошенный интерфейс операции.
        /// </summary>
        ServerUnknownOperationType = 2,

        /// <summary>
        /// Указывает, что найденный тип, реализующий запрошенный интерфейс операции, не соответствует внутренним правилам сервера.
        /// </summary>
        ServerWrongOperationType = 4,

        /// <summary>
        /// Указывает, что во время создания экземпляра операции возникла неожиданная ошибка.
        /// </summary>
        ServerErrorUntilOperationCreate = 8,

        /// <summary>
        /// Указывает, что во время выполнения дополнительного вызова (WithCall) возникла неожиданная ошибка.
        /// </summary>
        ServerErrorUntilOperationCall = 16,

        /// <summary>
        /// Указывает, что во время выполнения запроса на сервере возникла неожиданная ошибка, не предусмотренная другими кодами ошибок.
        /// </summary>
        ServerErrorUnknown = 32,

        /// <summary>
        /// Указывает, что сервер не смог найти запущенную операцию с указанным идентификатором.
        /// </summary>
        ServerUnknownOperationID = 64,

        /// <summary>
        /// Указывает, что тип операции с указанным идентификатором не соответствует найденному типу, реализующему запрошенный интерфейс операции.
        /// </summary>
        ServerMismatchOperationType = 128,

        /// <summary>
        /// Указывает, что один из типов в реальном ответе операции не соответствует типу, заявленному в контракте. 
        /// </summary>
        ServerMismatchContractType = 2048,

        /// <summary>
        /// Указывает, что клиенту не удалось подключиться к серверу по какой-то причине.
        /// </summary>
        ClientCannotConnect = 256,

        /// <summary>
        /// Указывает, что при передаче данных на сервер возникла сетевая ошибка, которая не обработана отдельно.
        /// </summary>
        ClientConnectError = 512,

        /// <summary>
        /// Указывает, что при передаче данных на сервер возникла ошибка, которая не обработана отдельно.
        /// </summary>
        ClientUnknownError = 1024,

    }
}
