<?php
//REST API
date_default_timezone_set('Europe/Istanbul');
$conn = new mysqli('localhost', 'id', 'pw', 'db');
$conn->set_charset("utf8");
if ($conn->connect_error) {
	die("errcode31" /*$conn->connect_error*/);//veritabanına bağlanırken hata oluştu
}

function winRates($prx){
	$winRate=0;
	if ($prx<5) $winRate=6;
	else if ($prx>=5 && $prx<7) $winRate=20;
	else if ($prx>=7 && $prx<9) $winRate=80;
	else if ($prx>=9 && $prx<11) $winRate=90;
	else if ($prx>=11 && $prx<13) $winRate=60;
	else if ($prx>=13 && $prx<20) $winRate=6;
	else if ($prx>=20 && $prx<35) $winRate=4;
	else if ($prx>=35 && $prx<40) $winRate=2;
	else if ($prx>=40 && $prx<50) $winRate=1;
	else $winRate=0;
	return $winRate;
}

	echo("[");

	$return="";
	$ac=$conn->prepare("select id,page_name,page_img from product_pages order by product_page_sort");
	$ac->execute();
	$ac=$ac->get_result();
	while($_sc = mysqli_fetch_assoc($ac)){

		$return2=
		'{'.
		'"pn":"0",'.
		'"pp":"0",'.
		'"pm":"c/i/0.jpg",'.
		'"pr":"13"'.
		'},';
				
		$ac2=$conn->prepare("select product_name,product_price,product_img from products where product_page_id=? and on_sale=1 order by product_price");
		$ac2->bind_param("s",$_sc["id"]);
		$ac2->execute();
		$ac2=$ac2->get_result();
		while($_sc2 = mysqli_fetch_assoc($ac2)){
			
			$r_product_price=$_sc2["product_price"];
			
			$winRate=winRates($r_product_price);
			
			if ($winRate!=0){
				$return2.=
				'{'.
				'"pn":"'.$_sc2["product_name"].'",'.
				'"pp":"'.$r_product_price.'",'.
				'"pm":"'.$_sc2["product_img"].'",'.
				'"pr":"'.$winRate.'"'.
				'},';
			}
		}
		if ($return2!=""){
			$return2=str_replace('"','--',$return2);
			$return2='['.substr($return2, 0, -1).']';
			$return.='{"pi":'.$_sc["id"].',"pn":"'.$_sc["page_name"].'","pm":"'.$_sc["page_img"].'","product":"'.$return2.'"},';
		}

	}
	$return=substr($return, 0, -1);
	echo($return);
	
	echo("]");

?>