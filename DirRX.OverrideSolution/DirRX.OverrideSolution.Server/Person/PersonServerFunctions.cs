using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.OverrideSolution.Person;

namespace DirRX.OverrideSolution.Server
{
  partial class PersonFunctions
  {

    /// <summary>
    /// Получить персону по СНИЛС.
    /// </summary>
    /// <param name="inila">СНИЛС.</param>
    /// <returns>Персона.</returns>
    [Public, Remote(IsPure = true)]
    public static IPerson GetPersonByINILA(string inila)
    {
      return People.GetAll().Where(x => x.INILA == inila).FirstOrDefault();
    }

  }
}