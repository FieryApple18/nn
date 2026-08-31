using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.OverrideSolution.Server
{
  public partial class ModuleFunctions
  {

    #region Заказное массовое подписание.

    /// <summary>
    /// Получить замещения текущего сотрудника.
    /// </summary>
    /// <returns>Список замещений.</returns>
    [Public, Remote(IsPure=true)]
    public IQueryable<IUser> GetCurrentUserActiveSubstitutions()
    {
      return Substitutions.GetAll()
        .Where(x => x.Status == Sungero.CoreEntities.DatabookEntry.Status.Active)
        .Where(x => x.IsSystem != true) // Исключить системные замещения.
        .Where(x => x.Substitute != null && x.Substitute.Equals(Users.Current))
        .Where(x => x.StartDate == null || x.StartDate.Value.Date <= Calendar.Now.Date)
        .Where(x => x.EndDate == null || x.EndDate.Value.Date >= Calendar.Now.Date)
        .Select(x => x.User);
    }

    /// <summary>
    /// Получить сертификаты для автоматического подписания.
    /// </summary>
    /// <returns>Сертификаты текущего пользователя.</returns>
    [Public, Remote(IsPure=true)]
    public IQueryable<ICertificate> GetCertificates()
    {
      var currentDate = Calendar.Now;
      
      return Certificates.GetAll()
        .Where(s => s.Owner.Equals(Users.Current) && s.Enabled == true)
        .Where(s => s.NotAfter.HasValue && s.NotAfter >= currentDate)
        .Where(s => s.NotBefore.HasValue && s.NotBefore <= currentDate);
    }

    /// <summary>
    /// Проверить блокировку на документе.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <returns>Текст блокировки или пустая строка.</returns>
    [Public, Remote(IsPure=true)]
    public static string CheckDocumentLock(DirRX.OverrideSolution.IOfficialDocument document)
    {
      var lockMessage = string.Empty;
      
      var lockInfo = Locks.GetLockInfo(document);
      
      if (lockInfo.IsLocked && lockInfo.IsLockedByOther)
      {
        lockMessage = DirRX.OverrideSolution.Resources.DocumentLockedMessageFormat(lockInfo.OwnerName, lockInfo.LockTime);
      }
      
      return lockMessage;
    }

    #endregion

  }
}