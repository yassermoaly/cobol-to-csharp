using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobolToCSharp
{
    public enum StatementType
    {
        NONE = 0,
        MOVE,
        IF,
        PERFORM,
        ELSE,
        ELSE_IF,
        END_IF,
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
        COMMENT,
        BEGIN_BLOCK,
        END_BLOCK,
        END_PROGRAM,
        STOP_RUN,
        INSPECT,
        ACCEPT
    }
}
