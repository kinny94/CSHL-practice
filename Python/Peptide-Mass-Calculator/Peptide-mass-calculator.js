const fs = require('fs');
const readline = require('readline');

// Amino acids constants
const AMINO_ACIDS_MASS = {
    
    "A": 71.037114,  "R": 156.101111,  "N": 114.042927,    "D": 115.026943	, "C": 103.009185, "Q": 128.058578,
    "E": 129.042593, "H": 137.058912, "I": 113.084064, "L": 113.084064, "K": 128.094963, "M": 131.040485, "G": 57.021464,
    "F": 147.068414, "P": 97.052764,  "S": 87.032028,  "T": 101.047679, "W": 186.079313, "Y": 163.06332, "V": 99.068414	, "U":150.95363
    
}

// Split function taking two arguments to split the protien at two positions
const split = ( protien ) => {
    let elements = [];
    let string = "";
    for( let i=0; i<protien.length; i++ ){     
        //console.log(protien[i]);
        if( protien[i] === "K" || protien[i] === "R" ){
            string += protien[i];
            elements.push( string );
            string = "";
        }else{
            string += protien[i];
        } 
    }
    
    if( string !== "" ){
        elements.push( string );
    }
    
    return elements;
}

// compute all peptides in a protien with their masses
const peptides_with_mass = ( protien ) => {
    let PEPTIDES = [];
    PEPTIDES = split( protien, "K", "R" );
    let PEPTITED_WITH_MASS = {};
    
    for( let i=0; i<PEPTIDES.length; i++ ){
        let mass = 0; 
        if( i == 0){
            mass += 1;
        }
        if( i == PEPTIDES.length - 1 ){
            mass += 18;
        }
        for( let j=0; j<PEPTIDES[i].length; j++ ){
            mass += AMINO_ACIDS_MASS[PEPTIDES[i][j]];
        }
        PEPTITED_WITH_MASS[PEPTIDES[i]] = mass;
    }
    return PEPTITED_WITH_MASS;
} 

// computer mass of entire protien
const compute_protien_mass = ( object_of_peptides ) => {
    let totalMass = 0;
    for( let key in object_of_peptides ){
        totalMass += object_of_peptides[ key ];
    }
    return totalMass
}

// Main Function
const run = ( pathToFile ) => {
    return new Promise(( resolve, reject ) => {
        let protiens_in_a_file = [];
        var results = [];
        const rd = readline.createInterface({
            input: fs.createReadStream( pathToFile ),
            console: false
        });
        
        var temp = [];
        rd.on('line', function(line) {
            let protien_string = "";
            if( line[0] !== ">"){
                temp.push(line);
            }else{
                let join = temp.join("");
                protiens_in_a_file.push( join );
                temp = [];
            }
        });
        
        var results = [];
        rd.on('close', ( line ) => {
            for( let i=1; i<protiens_in_a_file.length; i++ ){
                results.push( peptides_with_mass( protiens_in_a_file[i] ));
                // console.log(" *************************************************** ");
                // console.log( "Protien number : " + i );
                if( results.length === protiens_in_a_file.length - 1 ){
                    resolve( results );
                }
            }
            
        });
    });
}

run("UniProt_E.coli_20170509.fasta").then(( results ) => {
    console.log( results );
});

module.exports = run;
