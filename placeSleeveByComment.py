# Load the Python Standard and DesignScript Libraries

import clr
clr.AddReference("RevitAPI")
import Autodesk
from Autodesk.Revit.DB import *

clr.AddReference("RevitServices")
import RevitServices
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

clr.AddReference("RevitAPIUI")

doc = DocumentManager.Instance.CurrentDBDocument

# The inputs to this node will be stored as a list in the IN variables.
elements = UnwrapElement(IN[0])
level = UnwrapElement(IN[1])
two_in_sleeve = UnwrapElement(IN[2])
two_in_tb_sleeve = UnwrapElement(IN[3])
three_in_sleeve = UnwrapElement(IN[4])
four_in_aerator_sleeve = UnwrapElement(IN[5])
four_in_sleeve = UnwrapElement(IN[6])
four_in_wc_sleeve = UnwrapElement(IN[7])
six_in_sleeve = UnwrapElement(IN[8])
eight_in_sleeve = UnwrapElement(IN[9])
ten_in_sleeve = UnwrapElement(IN[10])
twelve_in_sleeve = UnwrapElement(IN[11])
elevation = UnwrapElement(IN[12])

out = []

# Place your code below this line
for e in elements:
	p = []
	sleeve = 0;
	origin = e.Origin
	origin_x = origin.X
	origin_y = origin.Y
	origin_z = origin.Z
	size = e.LookupParameter("Inside Diameter").AsDouble()
	comment = e.LookupParameter("Comments").AsString()

	if comment == "UP TO WC":
		sleeve = four_in_wc_sleeve
	elif comment == "UP TO SH":
		sleeve = six_in_sleeve
	elif comment == "UP TO TB":
		sleeve = two_in_tb_sleeve
	#elif comment == "AERATOR":
		#sleeve = four_in_aerator_sleeve
		#elevation = elevation - 4
	elif comment == "UP TO MC":
		sleeve = two_in_sleeve
	else:
		if size <= 0.175:
			sleeve = two_in_sleeve
		elif size <= 0.26:
			sleeve = three_in_sleeve
		elif size <= 0.35:
			sleeve = four_in_sleeve
		elif size <= 0.51:
			sleeve = six_in_sleeve
		elif size <= 0.668:
			sleeve = ten_in_sleeve
		elif size <= 2:
			sleeve = twelve_in_sleeve

	p.append(origin_x)
	p.append(origin_y)
	p.append(origin_z)
	p.append(level)
	p.append(size)
	p.append(comment)
	p.append(sleeve)
	p.append(e.Id)
	p.append(elevation)
	out.append(p)

#The code below adds the aerator adapter sleeve for any vertical pipes with Comment value "AERATOR"
for e in elements:
	comment = e.LookupParameter("Comments").AsString()
	if comment == "AERATOR":
		q = []
		sleeve = 0;
		origin = e.Origin
		origin_x = origin.X
		origin_y = origin.Y
		origin_z = origin.Z
		size = e.LookupParameter("Inside Diameter").AsDouble()
		sleeve = four_in_aerator_sleeve
		elevation = elevation - 4
		
		q.append(origin_x)
		q.append(origin_y)
		q.append(origin_z)
		q.append(level)
		q.append(size)
		q.append(comment)
		q.append(sleeve)
		q.append(e.Id)
		q.append(elevation)
		out.append(q)

# Assign your output to the OUT variable.
OUT = out
