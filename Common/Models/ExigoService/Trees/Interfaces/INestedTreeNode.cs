﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExigoService
{
    public interface INestedTreeNode<T> : ITreeNode
    {
        List<T> Children { get; set; }
    }
}