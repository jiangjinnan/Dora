using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Interception
{
  /// <summary>
  /// 
  /// </summary>
  public interface IAttributeCollection : IEnumerable<Attribute>
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    IAttributeCollection Add(Attribute attribute);


    /// <summary>
    /// 
    /// </summary>
    /// <param name="attributes"></param>
    /// <returns></returns>
    IAttributeCollection AddRange(IEnumerable<Attribute> attributes);
  }
}
