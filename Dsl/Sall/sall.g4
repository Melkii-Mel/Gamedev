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
l1Expr: ('-' | '+' | '!')* atom;
l2Expr: l1Expr (('*' | '/' | '%') l1Expr)*;
l3Expr: l2Expr (('+' | '-') l2Expr)*;
l4Expr: l3Expr (('<' | '>' | '==' | '!=') l3Expr)*;
l5Expr: l4Expr ('&&' l4Expr)*;
l6Expr: l5Expr ('||' l5Expr)*;
expr: l6Expr;

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
		(
			uiSelector
			| childrenSelector
			| parentSelector
			| siblingsSelector
		) stateMap?
	)
	| stateMap;
uiSelector: '@' '+'? IDENT;
childrenSelector: '>' ('(' uintRange ')')?;
parentSelector: '<' ('(' uintRange ')')?;
siblingsSelector: '-' ('(' uintRange ',' uintRange ')')?;
customSelector: IDENT params?;
stateMap: '[' state? (',' state)* ','? ']';
state: stateKvp | IDENT;
stateKvp: IDENT '=' expr;

call: IDENT args;
value: NUMBER | sizeValue | IDENT | COLOR | call;
sizeValue: NUMBER UNIT;
uintRange: UINT ('..' UNIT)?;

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

COLOR:
	'#' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT;
NUMBER: DIGITS '.'? DIGITS? | '.' DIGITS;
UINT: DIGITS;
UNIT: ('px' | '%' | 'em' | 'rem' | 'vh' | 'vw');

PERCENT: '%';

IDENT: [a-zA-Z_][a-zA-Z0-9_]*;

fragment DIGITS: [0-9]+;
fragment HEX_DIGIT: [0-9A-Fa-f];

WSNL: [ \t\r\n]+ -> skip;
WS: [ \t]+ -> skip;
COMMENT: '#' WS ~[\r\n]* -> skip;