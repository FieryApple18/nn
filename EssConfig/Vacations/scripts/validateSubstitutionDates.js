class validateSubstitutionDates {
  execute(args) {
    return new Promise((resolve, reject) => {
      try {
        var vacationPeriodsJson = args.VacationPeriodsJson;
        var startDatesString = args.StartDatesString;
        var endDatesString = args.EndDatesString;
        var isCallingService = args.IsCallingService || false;
        
        var errors = [];
        
        startDatesString = startDatesString || '';
        endDatesString = endDatesString || '';
        
        if (startDatesString === '' || endDatesString === '') {
          resolve({ isValid: true, errors: [] });
          return;
        }
        
        var vacationPeriods = [];
        
        if (vacationPeriodsJson && vacationPeriodsJson !== '' && vacationPeriodsJson !== 'null') {
          try {
            var periods = JSON.parse(vacationPeriodsJson);
            if (periods && periods.length > 0) {
              for (var i = 0; i < periods.length; i++) {
                var p = periods[i];
                var startDate = p.startDate || p.StartDate;
                var endDate = p.endDate || p.EndDate;
                var start = startDate ? new Date(startDate) : null;
                var end = endDate ? new Date(endDate) : null;
                vacationPeriods.push({
                  StartDate: start ? (start.getFullYear() + '-' + String(start.getMonth() + 1).padStart(2, '0') + '-' + String(start.getDate()).padStart(2, '0')) : null,
                  EndDate: end ? (end.getFullYear() + '-' + String(end.getMonth() + 1).padStart(2, '0') + '-' + String(end.getDate()).padStart(2, '0')) : null
                });
              }
            }
          } catch (e) {
            console.error('[validateSubstitutionDates] Parse vacationPeriodsJson error:', e);
          }
        }
        
        if (vacationPeriods.length === 0) {
          errors.push({ rowIndex: -1, field: 'StartDate', message: ESS.L('@HRPROVACATIONS.FACILITIES_NN_SUBSTITUTION_SETUP_DATE_OUT_OF_PERIOD').replace('{0}', '').replace('{1}', '') });
          resolve({ isValid: false, errors: errors });
          return;
        }
        
        var storedStartDates = startDatesString.split(';').filter(function(d) { return d && d !== ''; });
        var storedEndDates = endDatesString.split(';').filter(function(d) { return d && d !== ''; });
        
        if (storedStartDates.length !== storedEndDates.length) {
          errors.push({ rowIndex: -1, field: 'StartDate', message: ESS.L('@HRPROVACATIONS.FACILITIES_NN_SUBSTITUTION_SETUP_CHECK_ERROR') });
          resolve({ isValid: false, errors: errors });
          return;
        }
        
        for (var i = 0; i < storedStartDates.length; i++) {
          var subStartDate = storedStartDates[i] ? new Date(storedStartDates[i]) : null;
          var subEndDate = storedEndDates[i] ? new Date(storedEndDates[i]) : null;
          
          var subStart = subStartDate ? (subStartDate.getFullYear() + '-' + String(subStartDate.getMonth() + 1).padStart(2, '0') + '-' + String(subStartDate.getDate()).padStart(2, '0')) : null;
          var subEnd = subEndDate ? (subEndDate.getFullYear() + '-' + String(subEndDate.getMonth() + 1).padStart(2, '0') + '-' + String(subEndDate.getDate()).padStart(2, '0')) : null;
          
          if (!subStart || !subEnd) {
            continue;
          }
          
          if (subStart > subEnd) {
            if (!isCallingService) {
              errors.push({ rowIndex: i, field: 'StartDate', message: ESS.L('@HRPROVACATIONS.FACILITIES_NN_SUBSTITUTION_SETUP_START_AFTER_END') });
            }
            continue;
          }
          
          var isValidPeriod = false;
          var nearestPeriod = vacationPeriods[0];
          var startPeriodIndex = -1;
          var endPeriodIndex = -1;
          
          for (var j = 0; j < vacationPeriods.length; j++) {
            var period = vacationPeriods[j];
            if (subStart >= period.StartDate && subStart <= period.EndDate) {
              startPeriodIndex = j;
            }
            if (subEnd >= period.StartDate && subEnd <= period.EndDate) {
              endPeriodIndex = j;
            }
            if (subStart >= period.StartDate && subEnd <= period.EndDate) {
              isValidPeriod = true;
              break;
            }
            if (Math.abs(new Date(subStart) - new Date(period.StartDate)) < Math.abs(new Date(subStart) - new Date(nearestPeriod.StartDate))) {
              nearestPeriod = period;
            }
          }
          
          if (!isCallingService) {
            if (startPeriodIndex !== -1 && endPeriodIndex !== -1 && startPeriodIndex !== endPeriodIndex) {
              errors.push({ rowIndex: i, field: 'EndDate', message: ESS.L('@HRPROVACATIONS.FACILITIES_NN_SUBSTITUTION_SETUP_DATES_SPAN_PERIODS') });
              continue;
            }
          }
          
          if (!isValidPeriod) {
            var periodStartDisplay = new Date(nearestPeriod.StartDate).toLocaleDateString('ru-RU');
            var periodEndDisplay = new Date(nearestPeriod.EndDate).toLocaleDateString('ru-RU');
            var subStartDisplay = subStartDate.toLocaleDateString('ru-RU');
            
            if (isCallingService) {
              var errorMsg = ESS.L('@HRPROVACATIONS.FACILITIES_NN_SUBSTITUTION_SETUP_DATE_MISMATCH').replace('{0}', subStartDisplay).replace('{1}', periodStartDisplay + ' - ' + periodEndDisplay);
              errors.push({ rowIndex: i, field: 'StartDate', message: errorMsg });
              resolve({ isValid: false, errors: errors });
              return;
            } else {
              var errorMsg = ESS.L('@HRPROVACATIONS.FACILITIES_NN_SUBSTITUTION_SETUP_DATE_OUT_OF_PERIOD').replace('{0}', periodStartDisplay).replace('{1}', periodEndDisplay);
              
              var startInPeriod = false;
              var endInPeriod = false;
              
              for (var j = 0; j < vacationPeriods.length; j++) {
                var period = vacationPeriods[j];
                if (subStart >= period.StartDate && subStart <= period.EndDate) startInPeriod = true;
                if (subEnd >= period.StartDate && subEnd <= period.EndDate) endInPeriod = true;
              }
              
              if (!startInPeriod) {
                errors.push({ rowIndex: i, field: 'StartDate', message: errorMsg });
              }
              if (!endInPeriod) {
                errors.push({ rowIndex: i, field: 'EndDate', message: errorMsg });
              }
            }
          }
        }
        
        console.log('[validateSubstitutionDates] Result:', { isValid: errors.length === 0, errors: errors });
        resolve({ isValid: errors.length === 0, errors: errors });
        
      } catch (e) {
        console.error('[validateSubstitutionDates] Critical error:', e);
        resolve({ 
          isValid: false, 
          errors: [{ rowIndex: -1, field: 'StartDate', message: ESS.L('@HRPROVACATIONS.FACILITIES_NN_SUBSTITUTION_SETUP_CHECK_ERROR') }] 
        });
      }
    });
  }
}