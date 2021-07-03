" session title
set title titlestring=mediasoup-client

" formatting
set shiftwidth=2  " use indents of 2 spaces
" tabs are tabs
set noexpandtab     " tabs are spaces, not tabs
set tabstop=2     " an indentation every 2 columns
set softtabstop=2 " backspace delete indent
set backspace=2   " make backspace work

let g:ale_linters = {
\   'javascript': ['eslint'],
\   'typescript': ['eslint'],
\}

let g:ale_fixers = {
\    'typescript': ['prettier'],
\    'javascript': ['prettier'],
\    'markdown': [],
\}
