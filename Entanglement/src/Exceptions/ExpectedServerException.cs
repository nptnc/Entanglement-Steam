﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entanglement.Exceptions {
    public class ExpectedServerException : Exception {
        public override string Message => "ExpectedServerException: Client has received a Message which expects a Server.";
    }
}