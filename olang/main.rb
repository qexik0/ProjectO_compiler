require "ruby_grammar_builder"

grammar = Grammar.new(
    name: "OLang",
    scope_name: "source.olang",
    fileTypes: [
        "olang"
    ],
    version: "0.0.1"
)

grammar[:classDeclaration] = PatternRange.new(
    start_pattern: @word_boundary.then(match: /class/, tag_as: "keyword.control").then(@word_boundary),
    end_pattern: @word_boundary.then(match: /end/, tag_as: "keyword.control").then(@word_boundary),
    tag_as: "meta.class",
    includes: [
        :methodMeta,
        :classDeclName,
        :variableDecl,
        :constructorMeta
    ]
)

grammar[:className] = Pattern.new(
    match: @word_boundary.then(match: @word, tag_as: "entity.name.type").maybe(@spaces).maybe(Pattern.new(
        match: /\[/
        ).maybe(@spaces).then(match: @word, tag_as: "entity.name.type").maybe(@spaces).then(/\]/))
)

grammar[:classDeclName] = Pattern.new(
    match: lookBehindFor(/class/).then(@spaces).then(grammar[:className]).then(@spaces)
        .maybe(Pattern.new(match: /extends/, tag_as: "keyword.control").then(@spaces).then(grammar[:className]))
        .then(match: /is/, tag_as: "keyword.control")
)

grammar[:variableDecl] = Pattern.new(
    match: @word_boundary.then(match: /var/, tag_as: "keyword.control").then(@spaces)
        .then(match: /\w+/, tag_as: "variable.other").then(@spaces).then(/:/).then(match: /.*/, tag_as: "meta.expression")
)

grammar[:parameterDecl] = Pattern.new(
    @word_boundary.then(match: @word, tag_as: "variable.parameter").maybe(@spaces).then(/:/).maybe(@spaces)
        .then(grammar[:className]).maybe(@spaces)
)

grammar[:cparameterDecl] = Pattern.new(
    Pattern.new(/,/).maybe(@spaces).then(match: @word, tag_as: "variable.parameter").maybe(@spaces).then(/:/).maybe(@spaces)
        .then(grammar[:className]).maybe(@spaces)
)

grammar[:returnTypeDecl] = Pattern.new(
    Pattern.new(/:/).maybe(@spaces).then(grammar[:className])
)

grammar[:methodDecl] = Pattern.new(
    match: lookBehindFor(@word_boundary.then(/method/).then(@spaces)).then(match: @word, tag_as: "entity.name.function")
        .maybe(@spaces).maybe(Pattern.new(
            /\(/
            ).maybe(@spaces).then(grammar[:parameterDecl]).zeroOrMoreOf(grammar[:cparameterDecl]).then(/\)/)
        ).maybe(@spaces).maybe(grammar[:returnTypeDecl]).then(@spaces).then(match: /is/, tag_as: "keyword.control")
)

grammar[:methodMeta] = PatternRange.new(
    start_pattern: @word_boundary.then(match: /method/, tag_as: "keyword.control").then(@word_boundary),
    end_pattern: @word_boundary.then(match: /end/, tag_as: "keyword.control").then(@word_boundary),
    tag_as: "meta.function",
    includes: [
        :methodDecl,
        :body
    ]
)

grammar[:constructorMeta] = PatternRange.new(
    start_pattern: @word_boundary.then(match: /this/, tag_as: "variable.language").then(@word_boundary),
    end_pattern: @word_boundary.then(match: /end/, tag_as: "keyword.control").then(@word_boundary),
    tag_as: "meta.constructor",
    includes: [
        :constructorDecl,
        :body
    ]
)

grammar[:constructorDecl] = Pattern.new(
    match: lookBehindFor(@word_boundary.then(/this/).maybe(@spaces)).maybe(Pattern.new(
            /\(/
            ).maybe(@spaces).then(grammar[:parameterDecl]).zeroOrMoreOf(grammar[:cparameterDecl]).then(/\)/)
        ).maybe(@spaces).then(match: /is/, tag_as: "keyword.control")
)

grammar[:assignment] = Pattern.new(
    match: @word_boundary.then(match: @word, tag_as: "variable.other").maybe(@spaces)
        .then(match: /:=/, tag_as: "keyword.operator").maybe(@spaces).then(match: /.*/, tag_as: "meta.expression")
)

grammar[:whileMeta] = PatternRange.new(
    start_pattern: @word_boundary.then(match: /while/, tag_as: "keyword.control"),
    end_pattern: @word_boundary.then(match: /end/, tag_as: "keyword.control"),
    tag_as: "meta.whileloop",
    includes: [
        :whileLoop,
        :body
    ]
)

grammar[:whileLoop] = Pattern.new(
    match: lookBehindFor(/while/).then(match: /.*/, tag_as: "meta.expression").then(match: /loop/, tag_as: "keyword.control")
)

grammar[:body] = [
    :variableDecl,
    :assignment,
    :whileMeta,
    :ifMeta,
    :returnStatement
]

grammar[:returnStatement] = Pattern.new(
    match: @word_boundary.then(match: /return/, tag_as: "keyword.control").maybe(@spaces.then(match: /.*/, tag_as: "meta.expression"))
)

grammar[:ifMeta] = PatternRange.new(
    start_pattern: @word_boundary.then(match: /if/, tag_as: "keyword.control"),
    end_pattern: @word_boundary.then(match: /end/, tag_as: "keyword.control"),
    tag_as: "meta.ifstatement",
    includes: [
        :ifExpr,
        :elseKeyword,
        :body
    ]
)

grammar[:elseKeyword] = Pattern.new(
    match: @word_boundary.then(match: /else/, tag_as: "keyword.control").then(@word_boundary)
)

grammar[:ifExpr] = Pattern.new(
    match: lookBehindFor(/if/).then(match: /.*/, tag_as: "meta.expression").then(match: /then/, tag_as: "keyword.control")
)

grammar[:$initial_context] = [
    :classDeclaration
]

grammar.save_to(syntax_name: "olang", syntax_dir: "./syntaxes")