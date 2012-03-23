.RECIPEPREFIX = >
name := Libraries.Starlight.dll 
thisdir := .
cmd_library := -t:library
cmd_out := -out:$(name)
cmd_compiler := dmcs
sources := *.cs 
options := -d:GATHERING_STATS
result := $(name)

build: $(sources)
> dmcs -optimize $(cmd_library) $(cmd_out) $(sources)

debug: $(sources)
> dmcs -debug $(cmd_library) $(cmd_out) $(sources)

stats: $(result)
> dmcs $(options) $(cmd_library) $(cmd_out) $(sources)

.PHONY : clean
clean: 
> -rm -f $(result) *.mdb 
