CSharp['AC'] = CSharp['Assembly-CSharp']
CSharp['S'] = CSharp['Sanmon']

local function _new_list(type)
    local list_type = zlua.make_generic_type(
        CSharp.mscorlib['System.Collections.Generic.List`1'], type
    )
    return list_type()
end

local function _new_dict(key_type, value_type)
    local dict_type = zlua.make_generic_type(
        CSharp.mscorlib['System.Collections.Generic.Dictionary`2'], key_type, value_type
    )
    return dict_type()
end

return{
    ac = CSharp['AC'],
    framework = CSharp['S'],
    new_list = _new_list,
    new_dict = _new_dict,
}