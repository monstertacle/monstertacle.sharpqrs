# Additive public contracts

Existing ICommand/IQuery and asynchronous handler/dispatcher contracts remain intact.
ICommandRegistration and IQueryRegistration expose existing generic Register methods.
ISyncCommandHandler<TCommand,TResult> and ISyncQueryHandler<TQuery,TResult> expose
TResult Handle(request, CancellationToken=default), with contravariant request type.
ISyncCommandDispatcher and ISyncQueryDispatcher expose TResult Dispatch<TResult>(
ICommand<TResult>/IQuery<TResult>, CancellationToken=default). Corresponding sync
registration interfaces expose Register with the synchronous handler contract.
SyncCommandDispatcher/SyncQueryDispatcher implement their two mode-specific contracts.

Synchronous dispatch checks null request, exact mapping and exact result before one
invocation; null actual results fail. Original errors and cancellation propagate.
Finish setup before dispatch. Concurrent reads are supported; handler thread safety
is the application's responsibility. No cross-mode adaptation occurs.

Integration adds two-generic-argument AddSharpQrsCommand/Query overloads for existing
handler services, plus AddSharpQrsSynchronous and AddSharpQrsSyncCommand/Query with
contract-based or concrete-type selection. Exact descriptor selection is validated
at setup and dispatcher construction, without handler activation. Only unkeyed
services participate. Missing and ambiguous selections fail with diagnostics.
