import openpyxl, re
wb = openpyxl.load_workbook('spreadsheets/Team Dashboard.xlsx')
ws = wb['More Charts']

print(f'Total charts: {len(ws._charts)}')
for i, chart in enumerate(ws._charts):
    raw = repr(chart.title) if chart.title else ''
    titles = re.findall(r"v='([^']+)'", raw)
    titles += re.findall(r"t='([^']+)'", raw)
    title_text = ' / '.join(t for t in titles if t.strip()) if titles else 'Untitled'
    
    chart_type = type(chart).__name__
    
    series_info = []
    for j, s in enumerate(chart.series):
        s_raw = repr(s.title) if s.title else ''
        st = re.findall(r"v='([^']+)'", s_raw)
        s_title = st[0] if st else 'no title'
        
        val_ref = 'N/A'
        if s.val and hasattr(s.val, 'numRef') and s.val.numRef:
            val_ref = s.val.numRef.f
        
        series_info.append(f'{s_title} -> {val_ref}')
    
    print(f'\nChart {i+1}: [{chart_type}] "{title_text}"')
    for si in series_info:
        print(f'    {si}')
