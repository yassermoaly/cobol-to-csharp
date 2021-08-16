using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolParser
{
    public enum StatementType
    {
        NONE = 0,
        MOVE,
        IF,
        PERFORM,
        ELSE,
        DISPLAY,
        ADD,
        SUBTRACT,
        COMPUTE,
        CALL,
        DIVIDE,
        MULTIPLY,
        GOTO,
        EXITPROGRAM,
        QUERY,
        COMMENT
    }
}
