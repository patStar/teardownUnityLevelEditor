function init()    		
	alarmStarter = FindBodies("alarmstarter",true)	
end

function tick(dt)

	if not GetBool("level.alarm") then
		for i=1,#alarmStarter do
			if IsBodyOperated(alarmStarter[i]) then
				SetBool("level.alarm", true)
			end
		end
	end
  
end
