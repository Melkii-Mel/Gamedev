grammar sall;

/*
 * PARSER
 */

file: statement*;

statement: variable | namedClassDef | anonymousClassDef;

variable: 'let' IDENT params? '=' expr ';';

namedClassDef: className classContent;
anonymousClassDef: selectorExpr classContent;
classContent: (':' parentsList)? classBodyOrTerminator;
classBodyOrTerminator: classBody | ';';
classBody: '{' classBodyItem* '}';

atom: '(' expr ')' | value;
l1Expr: l1Op* atom;
l2Expr: l1Expr (l2Op l1Expr)*;
l3Expr: l2Expr (l3Op l2Expr)*;
l4Expr: l3Expr (l4Op l3Expr)*;
l5Expr: l4Expr (l5Op l4Expr)*;
l6Expr: l5Expr (l6Op l5Expr)*;
expr: l6Expr;

l1Op: '-' | '+' | '!';
l2Op: '*' | '/' | '%';
l3Op: '+' | '-';
l4Op: '<' | '>' | '==' | '!=';
l5Op: '&&';
l6Op: '||';

selectorAtom: '(' selectorExpr ')' | selector;
l1Sel: l1SelOp* selectorAtom;
l2Sel: l1Sel (l2SelOp l1Sel)*;
l3Sel: l2Sel (l3SelOp l2Sel)*;
l4Sel: l3Sel+;
selectorExpr: l4Sel;

l1SelOp: '!';
l2SelOp: '&&';
l3SelOp: '||';

params: '(' paramList? ')';
args: '(' expr (',' expr)* ','? ')';

classBodyItem: property ';' | anonymousClassDef;
classNameOrSelectorExpr: className | selectorExpr;
property: IDENT ':' expr;
paramList: param (',' param)* ','?;
param: IDENT '=' expr;
parent: IDENT args?;
className: IDENT params?;
parentsList: parent (',' parent)* ','?;
selector: uiSelector | markerSelector | stateMapSelector | axesSelector | sliceSelector | reverseSelector | uniqueSelector;
axesSelector: childrenSelector | parentSelector | leftSiblingsSelector | rightSiblingsSelector;
uiSelector: '@' IDENT;
childrenSelector: '>' sliceSelector?;
parentSelector: '<';
reverseSelector: '~';
uniqueSelector: '#';
leftSiblingsSelector: '-' sliceSelector?;
rightSiblingsSelector: '+' sliceSelector?;
stateMapSelector: '[' state? (',' state)* ','? ']';
sliceSelector: '[[' range ']]';
markerSelector: IDENT;
state: stateKvp | IDENT;
stateKvp: IDENT comp expr;

comp: EQ | NEQ | LESS | GREATER | LE | GE;

call: IDENT args;
value: uint | float | sizeValue | IDENT | COLOR | call | bool;
sizeValue: float UNIT;
range: boundedRange | rightUnboundedRange | leftUnboundedRange | pointRange;
boundedRange: expr '..' expr;
rightUnboundedRange: expr '..';
leftUnboundedRange: '..' expr;
pointRange: expr;

uint: DIGITS;
float: DIGITS '.'? DIGITS? | '.' DIGITS;
bool: TRUE | FALSE;

/*
 * LEXER
 */

LET: 'let';
TRUE: 'true';
FALSE: 'false';
DLBRACK: '[[';
DRBRACK: ']]';
INLINE_COMMENT_SCOPE: '##';

RANGE: '..';
ASSIGN: '=';
SEMICOLON: ';';
LBRACE: '{';
RBRACE: '}';
LPAREN: '(';
RPAREN: ')';
LBRACK: '[';
RBRACK: ']';
MINUS: '-';
PLUS: '+';
STAR: '*';
COLON: ':';
COMMA: ',';
EXCLAMATION: '!';
SLASH: '/';
LESS: '<';
GREATER: '>';
LE: '>=';
GE: '<=';
EQ: '==';
NEQ: '!=' | '<>';
AND: '&&';
OR: '||';
AT: '@';
DOT: '.';

COLOR:
	'#' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT;
UNIT: ('px' | '%' | 'em' | 'rem' | 'vh' | 'vw');

PERCENT: '%';

IDENT: [a-zA-Z_][a-zA-Z0-9_]*;

DIGITS: [0-9]+;
fragment HEX_DIGIT: [0-9A-Fa-f];

WSNL: [ \t\r\n]+ -> skip;
NL: [\r\n]+ -> skip;
WS: [ \t]+ -> skip;
COMMENT: '//' ~[\r\n]* -> skip;
INLINE_COMMENT: '/*' (.|[\r\n])*? '*/' -> skip;