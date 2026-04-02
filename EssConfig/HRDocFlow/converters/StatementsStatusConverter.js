class StatementsStatusConverter {
  convert(value, parameter, language) {
	  
	let statusModel = {};
	  
	if(language === 'ru')
    {		
      if(value === 'Черновик')
        statusModel = { title: value, fillColor: '#898989', name: 'Draft' };
      else if (value === 'На согласовании')
        statusModel = { title: value, fillColor: '#fcae48', name: 'InProcess' };
      else if (value === 'На доработке')
        statusModel = { title: value, fillColor: '#fcae48', name: 'InProcess' };
      else if (value === 'На исполнении')
        statusModel = { title: value, fillColor: '#fcae48', name: 'InProcess' };
      else if (value === 'Отказано')
        statusModel = { title: value, fillColor: '#f15045', name: 'Rejected' };
      else if (value === 'Прекращена')
        statusModel = { title: value, fillColor: '#f15045', name: 'Aborted' };
      else if (value === 'Завершена')
        statusModel = { title: value, fillColor: '#34b47d', name: 'Completed' };
    }
    else if(language === 'en')
    {
      if(value === 'Draft')
        statusModel = { title: value, fillColor: '#898989', name: 'Draft' };
      else if (value === 'Under approval')
        statusModel = { title: value, fillColor: '#fcae48', name: 'InProcess' };
      else if (value === 'On rework')
        statusModel = { title: value, fillColor: '#fcae48', name: 'InProcess' };
      else if (value === 'Being executed')
        statusModel = { title: value, fillColor: '#fcae48', name: 'InProcess' };
      else if (value === 'Rejected')
        statusModel = { title: value, fillColor: '#f15045', name: 'Rejected' };
      else if (value === 'Aborted')
        statusModel = { title: value, fillColor: '#f15045', name: 'Aborted' };
      else if (value === 'Completed')
        statusModel = { title: value, fillColor: '#34b47d', name: 'Completed' };
    }
	
    if (!statusModel.title)
		statusModel = converters.getStatusDescription(value);
	
	return `
	  <div style="
	    background-color: ${statusModel.fillColor};
		height: 8px;
		width: 8px;
		border-radius: 50%;
		margin: 4px;
		margin-left: 16px;
	  "></div>
	  ${statusModel.title}
	`;
  }
}