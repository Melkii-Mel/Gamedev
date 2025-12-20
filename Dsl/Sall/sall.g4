grammar sall;

/*
 * PARSER
 */

file: statement*;

statement: variable | classDef;

variable: 'let' IDENT params? '=' expr ';';

classDef: selector classContent;
subClassDef: subSelector classContent;
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

params: '(' paramList? ')';
args: '(' expr (',' expr)* ','? ')';

classBodyItem: property ';' | subClassDef;
property: IDENT ':' expr;
paramList: param (',' param)* ','?;
param: IDENT '=' expr;
parent: IDENT args?;
parentsList: parent (',' parent)* ','?;
selector: (uiSelector | customSelector) stateMap? subSelector?;
subSelector: (
		uiSelector
		| childrenSelector
		| parentSelector
		| siblingsSelector
	) stateMap?;
uiSelector: '@' '+'? IDENT;
childrenSelector: '>' ('(' uintRange ')')?;
parentSelector: '<' ('(' uintRange ')')?;
siblingsSelector: '-' ('(' uintRange ',' uintRange ')')?;
customSelector: IDENT params?;
stateMap: '[' state? (',' state)* ','? ']';
state: stateKvp | IDENT;
stateKvp: IDENT '=' expr;

call: IDENT args;
value: uint | float | sizeValue | IDENT | COLOR | call;
sizeValue: float UNIT;
uintRange: uint ('..' uint)?;

uint: DIGITS;
float: DIGITS '.'? DIGITS? | '.' DIGITS;

/*
 * LEXER
 */

LET: 'let';
RANGE: '..';
ASSIGN: '=';
SEMICOLON: ';';
LBRACE: '{';
RBRACE: '}';
LPAREN: '(';
RPAREN: ')';
MINUS: '-';
PLUS: '+';
STAR: '*';
COLON: ':';
COMMA: ',';
EXCLAMATION: '!';
SLASH: '/';
LESS: '<';
GREATER: '>';
EQ: '==';
NEQ: '!=';
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
WS: [ \t]+ -> skip;
COMMENT: '#' WS ~[\r\n]* -> skip;